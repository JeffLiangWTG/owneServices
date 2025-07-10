using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	public class StorageDocsBaseValueObjectDataAdatper : ValueObjectDataAdapter<BusinessObject, Xsd.DocumentMessage>
	{
		public override string RootCollectionElementName
		{
			get { return "DocumentMessages"; }
		}

		public override string RootElementName
		{
			get { return "DocumentMessage"; }
		}

		public override System.Xml.Schema.XmlSchema Schema
		{
			get { return XmlSchemaDefinitions.Instance.SingleDocumentMessageSchema; }
		}

		public override System.Xml.Schema.XmlSchema CollectionSchema
		{
			get { return XmlSchemaDefinitions.Instance.DocumentMessagesSchema; } // null so far. probably needs to be changed
		}

		protected override void ImportFromValueObjectCore(BusinessObject bizObj, Xsd.DocumentMessage value, Enterprise.DataTransfer.Integration.IValueObjectImportContext context)
		{
			throw new NotImplementedException();
		}

		protected override void ExportToValueObjectCore(BusinessObject bizObj, Xsd.DocumentMessage constructedValueObject, Enterprise.DataTransfer.Integration.IValueObjectExportContext context)
		{
			var result = constructedValueObject;

			IeDoc eDoc = (IeDoc)bizObj;
			if (eDoc != null)
			{
				IStorageMain storageMain = eDoc.ParentMain as IStorageMain;
				BusinessObject documentOwner = null;
				if (eDoc != null && storageMain != null)
				{
					documentOwner = storageMain.DocumentOwner;
				}

				if (eDoc.ImageData.Length > 0)
				{
					result.Document.FileName = eDoc.FileNameOnly;
					result.Document.Description = eDoc.Description;
					result.Document.Date = eDoc.DateAdded;
					result.Document.Data = eDoc.ImageData;
					result.Document.DocumentType = eDoc.DocType;
					result.Document.IsPublished = (eDoc.IsPublished) ? TrueFalse.@true : TrueFalse.@false;
					result.Document.IsPublishedSpecified = true;
					result.Document.DataType = eDoc.DataType;
					result.Document.IsSystemGenerated = (eDoc.IsSystemGenerated) ? TrueFalse.@true : TrueFalse.@false;
					result.Document.IsSystemGeneratedSpecified = true;

					if (storageMain != null && documentOwner != null)
					{
						Enterprise.MasterFiles.Business.OrgContact contactForEDocExport = GetContactForEDocExport(documentOwner.Factory);
						ZString link = (Globals.IsTest) ?
							"http://some.webaddress.com" :
							(TrackingUrlCreator.Instance.CreateUrl(contactForEDocExport != null ? contactForEDocExport.PK : ZGuid.Empty,
														TrackingConstants.BusinessContext.eDoc,
														eDoc.UniqueKey,
														storageMain.ParentFK));
						if (string.IsNullOrEmpty(link))
						{
							result.DocumentLink.Link = Res.GetString("b396985a-b108-4e33-9d64-49513e120efd", "Registry item 'System->Data Export Settings->Contact for eDoc Export' should be set up to enable eDoc's link generating");
						}
						else
						{
							result.DocumentLink.Link = link;
						}
					}

					PopulateReferenceKeys(documentOwner, result);
				}

				if (eDoc != null && documentOwner != null)
				{
					Xsd.Event eventToAdd = result.Events.Event.AddNew();
					eventToAdd.Code = "ADD";
					eventToAdd.Source = documentOwner.HumanReadableName;

					(documentOwner as EnterpriseBusinessObject).Logs.AddNew(Enterprise.ZArchitecture.Business.Events.DataExport, "eDoc: " + eDoc.FileName);
				}
			}
		}

		void PopulateReferenceKeys(BusinessObject documentOwner, Xsd.DocumentMessage documentMessage)
		{
			if (documentOwner is Enterprise.Integration.Customs.ICusEntryHeader || documentOwner is Enterprise.Integration.Customs.IBaseJobDeclaration)
			{
				var entryHeader = documentOwner as Enterprise.Integration.Customs.ICusEntryHeader;
				var declaration = documentOwner as Enterprise.Integration.Customs.IBaseJobDeclaration;

				if (entryHeader != null)
				{
					declaration = (Enterprise.Integration.Customs.IBaseJobDeclaration)documentOwner.Factory.Load(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)), entryHeader.CH_JE);
				}

				if (declaration != null)
				{
					PopulateReferenceKeysForDeclaration(declaration, documentMessage);
				}
			}
			else
			{
				IJobInvoicingPlugIn jobInvoicingPlugIn = documentOwner as IJobInvoicingPlugIn;
				if (jobInvoicingPlugIn != null)
				{
					PopulateReferenceKeysForJobInvoicingPlugIn(jobInvoicingPlugIn, documentMessage);
				}
			}
		}

		void PopulateReferenceKeysForJobInvoicingPlugIn(IJobInvoicingPlugIn jobInvoicingPlugIn, Xsd.DocumentMessage documentMessage)
		{
			if (!string.IsNullOrEmpty((jobInvoicingPlugIn.InvoicingSupporter.HouseBillNumber)))
			{
				DocumentMessageReferenceKey key = documentMessage.ReferenceKeys.AddNew();
				key.ReferenceKeyType = jobInvoicingPlugIn.InvoicingSupporter.HouseBillNumber;
				key.ReferenceKeyName = ReferenceType.HouseBill;
			}

			if (!string.IsNullOrEmpty((jobInvoicingPlugIn.InvoicingSupporter.MasterBillNumber)))
			{
				DocumentMessageReferenceKey key = documentMessage.ReferenceKeys.AddNew();
				key.ReferenceKeyType = jobInvoicingPlugIn.InvoicingSupporter.MasterBillNumber;
				key.ReferenceKeyName = ReferenceType.MasterBill;
			}

			if (!string.IsNullOrEmpty((jobInvoicingPlugIn.JobNumber)))
			{
				DocumentMessageReferenceKey key = documentMessage.ReferenceKeys.AddNew();
				key.ReferenceKeyType = jobInvoicingPlugIn.JobNumber;

				if (jobInvoicingPlugIn as Enterprise.Integration.Forwarding.IForwardingShipment != null)
				{
					key.ReferenceKeyName = ReferenceType.ShipmentJobNumber;
				}

				if (jobInvoicingPlugIn as Enterprise.Integration.Forwarding.IForwardingConsol != null)
				{
					key.ReferenceKeyName = ReferenceType.ConsolNumber;
				}

				if (jobInvoicingPlugIn as Enterprise.Integration.Forwarding.IOrder != null)
				{
					key.ReferenceKeyName = ReferenceType.OrderNumber;
				}
			}
		}

		void PopulateReferenceKeysForDeclaration(Enterprise.Integration.Customs.IBaseJobDeclaration declaration, Xsd.DocumentMessage documentMessage)
		{
			if (!declaration.JE_HouseBill.IsEmpty)
			{
				DocumentMessageReferenceKey key = documentMessage.ReferenceKeys.AddNew();
				key.ReferenceKeyType = declaration.JE_HouseBill;
				key.ReferenceKeyName = ReferenceType.HouseBill;
			}

			if (!declaration.JE_MasterBill.IsEmpty)
			{
				DocumentMessageReferenceKey key = documentMessage.ReferenceKeys.AddNew();
				key.ReferenceKeyType = declaration.JE_MasterBill;
				key.ReferenceKeyName = ReferenceType.MasterBill;
			}

			if (!declaration.JE_DeclarationReference.IsEmpty)
			{
				DocumentMessageReferenceKey key = documentMessage.ReferenceKeys.AddNew();
				key.ReferenceKeyType = declaration.JE_DeclarationReference;
				key.ReferenceKeyName = ReferenceType.DeclarationJobNumber;
			}
		}

		Enterprise.MasterFiles.Business.OrgContact GetContactForEDocExport(BusinessObjectFactory factory)
		{
			ZGuid contactPK = SystemDataRegistry.Instance.ContactForEDocExport.Value;
			return factory.Load<Enterprise.MasterFiles.Business.OrgContact>(contactPK);
		}

		public override XmlInterchange ToXmlInterchange(IList bizObjs, IValueObjectExportContext context)
		{
			if (bizObjs.Count == 0)
			{
				throw new ArgumentException("You must pass at least 1 business object.");
			}

			XmlInterchange result = XmlInterchange.NewPopulatedInterchange(((IeDoc)bizObjs[0]).ParentMain.Factory);
			result.Payload = new Payload { Data = bizObjs, Context = context, DataAdapter = this };
			return result;
		}
	}
}
