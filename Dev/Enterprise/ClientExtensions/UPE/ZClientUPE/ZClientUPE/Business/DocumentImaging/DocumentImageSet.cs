using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.DocumentImaging
{
	public class DocumentImageSet
	{
		public DocumentImageSet(DocumentIndexFileObject documentIndexFile, INotifications notifications, INotifications emailedNotifications)
		{
			this.documentIndexFileObject = documentIndexFile;
			this.notifications = notifications;
			this.emailedNotifications = emailedNotifications;
		}

		#region ImportAndDelete
		protected
 internal DocumentIndexFileObject Import()
		{
			DocumentIndexFileObject result = null;
			if (!documentIndexFileObject.IsIndexFileOld)
			{
				result = ImportDocument();
				if (result != null)
				{
					DeleteImageFiles();
				}
			}
			else
			{
				DeleteIndexFile();
				DeleteImageFiles();
			}
			return result;
		}

		DocumentIndexFileObject ImportDocument()
		{
			DocumentIndexFileObject result = null;
			IDocManagerSupport bizObj = GetBusinessObjectToAttachTo();

			if (bizObj != null && documentIndexFileObject.DocumentType != null && documentIndexFileObject.ImageFiles != null && documentIndexFileObject.ImageFiles.Count != 0)
			{
				if (AddDocumentPagesToJob(bizObj))
				{
					if (documentIndexFileObject.DocumentType.MoveJobToClassOnImport)
					{
						MoveJobToClassificationIfRequired((IProcessQueueParent)bizObj);
					}

					if (DeleteIndexFile())
					{
						BusinessObjectFactory.SaveTogether(Factory, bizObj.DocManagerInfo.MasterFactory);
						result = documentIndexFileObject;
						NotifyDocumentImported();
					}
				}
			}
			return result;
		}

		bool AddDocumentPagesToJob(IDocManagerSupport bizObj)
		{
			bool result = false;

			if (documentIndexFileObject.FirstImageBinary.Length > 0)
			{
				Bitmap newImage = null;

				using (var stream = new CargoWise.IO.Shim.SubStreamableStream())
				{
					for (int i = 0; i < documentIndexFileObject.ImageFiles.Count; i++)
					{
						try
						{
							var oldBitMap = new Bitmap(documentIndexFileObject.ImageFiles[i].FullName);
							var clonedBitMap = new Bitmap(oldBitMap.Width, oldBitMap.Height);
							var oldGraphics = Graphics.FromImage(clonedBitMap);
							oldGraphics.DrawImage(oldBitMap, CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledRectangle(0, 0, clonedBitMap.Width, clonedBitMap.Height));
							oldBitMap.Dispose();
							oldBitMap = clonedBitMap;

							if (newImage == null)
							{
								newImage = new Bitmap(oldBitMap, oldBitMap.Width, oldBitMap.Height);
								newImage.Save(stream, FileSaveHelper.GetTiffEncoder(), GetEncoderParameters(EncoderValue.MultiFrame));
							}
							else
							{
								newImage.SaveAdd(oldBitMap, GetEncoderParameters(EncoderValue.FrameDimensionPage));
							}

							oldGraphics.Dispose();
							oldBitMap.Dispose();
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							emailedNotifications.Notify(new InfoNotification(string.Format("Unable to import file ({0}) for shipment ({1}) because: {2} - {3}", documentIndexFileObject.ImageFiles[i].FullName, documentIndexFileObject.HouseBill, ex.Message, ex.StackTrace)));
							DeleteImage(newImage);
						}
					}

					if (newImage != null)
					{
						stream.Position = 0;
						StorageDocs document = (StorageDocs)bizObj.DocManagerInfo.AddFileOrDocument(stream, documentIndexFileObject.FirstTiffImageFilename, documentIndexFileObject.DocumentType.DocTypeCode);
						document.SC_Desc = documentIndexFileObject.DocumentType.Description;
						DeleteImage(newImage);
					}
				}

				Thread.Sleep(0);
				result = true;
			}
			return result;
		}

		void DeleteImage(Image image)
		{
			image.SaveAdd(GetEncoderParameters(EncoderValue.Flush));
			image.Dispose();
		}

		EncoderParameters GetEncoderParameters(EncoderValue value)
		{
			EncoderParameters @params = new EncoderParameters();
			@params.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)value);
			return @params;
		}

		void NotifyDocumentImported()
		{
			INotifications notifierForWhenImported = documentIndexFileObject.DocumentType.NotifyOnImport ? emailedNotifications : notifications;
			notifierForWhenImported.Notify(new InfoNotification(string.Format(documentProcessMessageFormat, documentIndexFileObject.DocumentType.Description, documentIndexFileObject.HouseBill)));
		}
		const string documentProcessMessageFormat = "Document of type '{0}' processed for house bill '{1}'.";

		bool DeleteIndexFile()
		{
			return IOExecuter.ExecuteIOFileTask(DeleteFileDelegate, documentIndexFileObject.IndexFile);
		}

		bool DeleteImageFiles()
		{
			bool result = true;
			if (documentIndexFileObject != null && documentIndexFileObject.ImageFiles != null)
			{
				foreach (FileInfo imageFile in documentIndexFileObject.ImageFiles)
				{
					result |= IOExecuter.ExecuteIOFileTask(DeleteFileDelegate, imageFile);
				}
			}
			return result;
		}

		static void DeleteFile(FileInfo file)
		{
			file.Delete();
		}

		#endregion

		#region MoveJobToClassificationIfRequired

		void MoveJobToClassificationIfRequired(IProcessQueueParent processQueueParent)
		{
			if (processQueueParent.CurrentQueue.P4_CustomsQueue == DeclarationQueueCodeDescriptionPairList.Codes.BCA ||
				processQueueParent.CurrentQueue.P4_CustomsQueue == DeclarationQueueCodeDescriptionPairList.Codes.BCO ||
				processQueueParent.CurrentQueue.P4_CustomsQueue == DefaultQueueCodeDescriptionPairList.Codes.EIR)
			{
				if ((IsCommercialInvoiceDocument && processQueueParent.CurrentQueue.P4_CustomsStatus == ReasonCodeDescriptionPairList.Codes.NY_MissingInvoice) ||
					(processQueueParent.CurrentQueue.P4_CustomsStatus == ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration) ||
					(processQueueParent.CurrentQueue.P4_CustomsStatus == ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient))
				{
					processQueueParent.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Classification;
				}
			}
		}

		bool IsCommercialInvoiceDocument
		{
			get { return (documentIndexFileObject.DocumentType.UPSCode == DocumentImageType.CommercialInvoiceUPSCode); }
		}

		#endregion

		#region GetBusinessObjectToAttachTo

		protected IDocManagerSupport GetBusinessObjectToAttachTo()
		{
			IDocManagerSupport result = null;
			BusinessObject[] matches = GetMatchingBusinessObjects();
			if (matches.Length == 0)
			{
				emailedNotifications.Notify(new InfoNotification(string.Format(couldNotFindJobMessageFormat, documentIndexFileObject.HouseBill, documentIndexFileObject.IndexFile.Name)));
			}
			else if (matches.Length >= 2)
			{
				emailedNotifications.Notify(new InfoNotification(string.Format(duplicateMatchesFoundMessageFormat, documentIndexFileObject.HouseBill, documentIndexFileObject.IndexFile.Name)));
			}
			else
			{
				result = (IDocManagerSupport)matches[0];
			}
			return result;
		}
		const string couldNotFindJobMessageFormat = "Could not find job with house bill '{0}' for index file '{1}'";
		const string duplicateMatchesFoundMessageFormat = "Duplicate matches found for house bill '{0}' for index file '{1}'. This document must be imported manually.";

		BusinessObject[] GetMatchingBusinessObjects()
		{
			BusinessObject[] result = GetDeclarationMatches();
			if (result.Length == 0)
			{
				result = GetCusHAWBMatches();
			}
			return result;
		}

		UPEJobDeclaration[] GetDeclarationMatches()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JobDeclarationSchema.JE_HouseBill, SQLComparisonOperator.Equal, documentIndexFileObject.HouseBill);
			filter.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_AgentsReference, SQLComparisonOperator.Equal, documentIndexFileObject.HouseBill);
			return Factory.Load<UPEJobDeclaration>(filter);
		}

		UPECusHAWB[] GetCusHAWBMatches()
		{
			List<UPECusHAWB> shipments = new List<UPECusHAWB>();
			// todo - is this still neccessary "2 queries required because SQL server 2000 doesn't optimise the query correctly"?
			shipments.AddRange(GetCusHAWBMatches_ForShortHAWB());
			if (shipments.Count == 0)
			{
				shipments.AddRange(GetCusHAWBMatches_ForCS_HAWB());
			}
			return GetNonSubsequentSplitShipments(shipments.ToArray());
		}

		UPECusHAWB[] GetCusHAWBMatches_ForCS_HAWB()
		{
			return Factory.Load<UPECusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, documentIndexFileObject.HouseBill));
		}

		UPECusHAWB[] GetCusHAWBMatches_ForShortHAWB()
		{
			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(CusHAWB));
			ZDBOnlySubQuery shortWayBillSubQuery = new ZDBOnlySubQuery(typeof(JobRelatedWayBill), JobRelatedWayBillSchema.EB_ParentID);
			shortWayBillSubQuery.AddToFilter(JoinCondition.And, JobRelatedWayBillSchema.EB_WaybillType, SQLComparisonOperator.Equal, JobRelatedWayBill.Constants.RelatedWayBillType.Parent);
			shortWayBillSubQuery.AddToFilter(JoinCondition.And, JobRelatedWayBillSchema.EB_WaybillShortNumber, SQLComparisonOperator.Equal, documentIndexFileObject.HouseBill);
			filter.AddSubQuery(shortWayBillSubQuery, JoinCondition.And);
			return Factory.Load<UPECusHAWB>(filter);
		}

		UPECusHAWB[] GetNonSubsequentSplitShipments(UPECusHAWB[] shipments)
		{
			List<UPECusHAWB> result = new List<UPECusHAWB>();
			foreach (UPECusHAWB shipment in shipments)
			{
				if (shipment.CurrentQueue.P4_Status != ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment)
				{
					result.Add(shipment);
				}
			}
			return result.ToArray();
		}

		#endregion

		IOFileTaskDelegate DeleteFileDelegate
		{
			get { return deleteFileDelegate ?? (deleteFileDelegate = new IOFileTaskDelegate(DeleteFile)); }
		}
		IOFileTaskDelegate deleteFileDelegate;

		NonPersistentIOExecuter IOExecuter
		{
			get { return ioExecuter ?? (ioExecuter = new NonPersistentIOExecuter(notifications, emailedNotifications)); }
		}
		NonPersistentIOExecuter ioExecuter;

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		readonly DocumentIndexFileObject documentIndexFileObject;
		readonly INotifications notifications;
		readonly INotifications emailedNotifications;
	}
}
