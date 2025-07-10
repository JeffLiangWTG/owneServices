using System;
using System.Diagnostics;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentScanning.DataTransfer.Universal
{
	[UniversalDataContext(DataContextType.DocManager)]
	public class DocManagerWrapper : NonPersistentBusinessObject, IDocManagerSupportCore, IStmALogParent, IJobNumber
	{
		public DocManagerWrapper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region IDocManagerSupportCore

		IDocManagerInfoCore IDocManagerSupportCore.DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfoCore(Factory));
		IDocManagerInfoCore docManagerInfo;

		#endregion

		#region IStmALogParent

		ZGuid IStmALogParent.LogsParentPK => PK;

		string IStmALogParent.LogsParentTableName => "DocManager";

		BusinessObject[] IStmALogParent.BusinessObjectsWithRelatedEvents => Array.Empty<BusinessObject>();

		bool IStmALogParent.DeferFiringWorkflow => true;

		Logs IStmALogProvider.Logs => logs ?? (logs = new Logs(this));
		Logs logs;

		BusinessObjectFactory IStmALogProvider.LogsFactory => Factory;

		#endregion

		#region IJobNumber

		public string JobNumber => string.Empty;

		#endregion

		#region DocManagerInfoCore

		class DocManagerInfoCore : IDocManagerInfoCore, ISimpleLoggerSupporter
		{
			public DocManagerInfoCore(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			readonly BusinessObjectFactory factory;

			public ISimpleLogger Logger { get; set; }

			public IDocumentFactory MasterFactory
			{
				get
				{
					if (masterFactory == null)
					{
						masterFactory = new DocumentFactoryProvider().GetFactory(factory ?? new BusinessObjectFactory());
						if (Logger != null)
						{
							((ILogSource)masterFactory).Log += ImportManager_LogProgress;
						}
					}

					return masterFactory;
				}
			}

			IDocumentFactory masterFactory;

			BatchImportManager ImportManager
			{
				get
				{
					if (importManager == null)
					{
						importManager = new BatchImportManager();
						if (Logger != null)
						{
							importManager.LogProgress += ImportManager_LogProgress;
						}
					}

					return importManager;
				}
			}
			BatchImportManager importManager;

			public IeDocBase AddFileOrDocument(byte[] contents, string filenameOnly, string documentType)
			{
				return AddFileOrDocument(contents, filenameOnly, documentType, false, Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			}

			IeDocBase AddFileOrDocument(
				byte[] contents,
				string filenameOnly,
				string documentType,
				bool overwriteExistingFileIfNotImageFile,
				Guid visibleCompanyPK,
				Guid visibleBranchPK,
				Guid visibleDepartmentPK,
				string documentSource)
			{
				if (((IBatchImportManagerInternals)ImportManager).ImportFileContent((DocumentFactory)MasterFactory, contents, filenameOnly, documentType, visibleCompanyPK, visibleBranchPK, visibleDepartmentPK))
				{
					if (ImportManager.HasPrefixInformation(filenameOnly))
					{
						var prefixInformation = new ZString(filenameOnly).SubstringSafe(0, filenameOnly.IndexOf(']'));
						try
						{
							var codeRetriever = new AllocationCodeRetriever((DocumentFactory)masterFactory, prefixInformation, true);
							documentType = codeRetriever.DocType;
							Logger.Log(LogType.Information, Res.GetString("d656517f-2124-4d8d-88da-102823f4575f", "Allocation details were detected in the filename and will be used in place of the Document Type mapping."));
						}
						catch (AllocationCodeFormatException) { }
					}

					var eDoc = new EDocBaseStub
					{
						FileNameOnly = filenameOnly,
						DocType = documentType,
						VisibleCompanyCode = visibleCompanyPK == Guid.Empty ? ZString.Empty : MasterFactory.FactoryForEverythingExceptEDocs.Load<GlbCompany>(visibleCompanyPK)?.GC_Code ?? ZString.Empty,
						VisibleBranchCode = visibleBranchPK == Guid.Empty ? ZString.Empty : MasterFactory.FactoryForEverythingExceptEDocs.Load<GlbBranch>(visibleBranchPK)?.GB_Code ?? ZString.Empty,
						VisibleDepartmentCode = visibleDepartmentPK == Guid.Empty ? ZString.Empty : MasterFactory.FactoryForEverythingExceptEDocs.Load<GlbDepartment>(visibleDepartmentPK)?.GE_Code ?? ZString.Empty
					};
					return eDoc;
				}

				if (Logger != null)
				{
					Log(LogType.Error, Res.GetString("b146614d-0b8f-4d9c-a9ae-bdb3b5264f3f", "Document {0} was not imported.", filenameOnly));
				}

				return null;
			}

			public IeDocBase AddFileOrDocument(SubStreamableStream contents, string filenameOnly, string documentType)
			{
				return AddFileOrDocumentCore(contents, filenameOnly, documentType, false, Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			}

			public IeDocBase AddFileOrDocumentCore(
				SubStreamableStream contents,
				string filenameOnly,
				string documentType,
				bool overwriteExistingFileIfNotImageFile,
				Guid visibleCompanyPK,
				Guid visibleBranchPK,
				Guid visibleDepartmentPK,
				string documentSource)
			{
				// TODO: would be better if streamed all the way
				var byteArray = AttachmentDef.StreamToByteArray(contents);
				contents.Dispose();
				return AddFileOrDocument(
					byteArray,
					filenameOnly,
					documentType,
					overwriteExistingFileIfNotImageFile,
					visibleCompanyPK,
					visibleBranchPK,
					visibleDepartmentPK,
					documentSource);
			}

			public void Save()
			{
				masterFactory?.Save();
			}

			public bool UseBusinessEntityFactoryAsInternal { get; set; }

			#region Logs

			void ImportManager_LogProgress(LogEventArgs e)
			{
				Log(e.EventType, e.Message);
			}

			void Log(TraceEventType eventType, string message)
			{
				LogType logType;
				switch (eventType)
				{
					case TraceEventType.Critical:
					case TraceEventType.Error:
						logType = LogType.Error;
						break;
					case TraceEventType.Warning:
						logType = LogType.Warning;
						break;
					case TraceEventType.Information:
						logType = LogType.Information;
						break;
					default:
						logType = LogType.Debug;
						break;
				}

				Log(logType, message);
			}

			void Log(LogType logType, string message)
			{
				if (Logger != null && logType == LogType.Error)
				{
					Logger.Log(logType, message);
				}
			}

			#endregion
		}
		#endregion

		#region EDocBaseStub

		sealed class EDocBaseStub : IeDoc
		{
			public void NotifyReadByUser()
			{
				throw new NotImplementedException();
			}

			public void Delete()
			{
				throw new NotImplementedException();
			}

			public ZBlob ImageData { get; set; }

			public ZBool IsDeleted { get; set; }

			public ZBool IsPublished { get; set; }

			public ZBool IsSystemGenerated => false;

			public ZDateTime DateAdded { get; set; } = ZDateTime.Now;

			public ZDateTime LastEdited { get; } = ZDateTime.Now;

			public ZString LastEditedUser { get; } = GlbStaff.CurrentUser.GS_Code;

			public ZGuid UniqueKey { get; } = ZGuid.NewZGuid();

			public ZString Description { get; set; }

			public ZString DocType { get; set; }

			public ZString DocSource { get; set; }

			public ZString DocSourceDescription { get; } = ZString.Empty;

			public ZString FileName => FileNameOnly;

			public ZString DataType => ZString.Empty;

			public ZString FileNameOnly { get; set; }

			public void SetValuesForTest(ZDateTime dateTime, ZString dataType) { }

			public IDisposable OpenForEdit()
			{
				throw new NotImplementedException();
			}

			public CodeDescriptionPairList DocType_List => new CodeDescriptionPairList();

			public BusinessObject ParentMain => null;

			public ZString VisibleCompanyCode { get; set; }

			public ZString VisibleBranchCode { get; set; }

			public ZString VisibleDepartmentCode { get; set; }

			public ZBool IsCustomisableDocTypes => ZBool.False;

			public ZDecimal FileSizeInMB { get; set; }

			public Stream GetImageDataReader()
			{
				throw new NotImplementedException();
			}

			public void SetImageDataStream(Stream stream)
			{
				throw new NotImplementedException();
			}

			public string CreateReference()
			{
				return StorageDocsBase.CreateReference(UniqueKey, DocType, DocSource);
			}
		}

		public void ProcessLog(IStmALog log)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
