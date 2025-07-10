using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[Serializable]
	public class QuarantineRemotePrintDeleterLogWalker : LogSubscriber
	{
		public override string FriendlyName
		{
			get { return "AU quarantine remote print deleter"; }
		}

		public override string Name
		{
			get { return "AuQrpDel"; }
		}

		public override string[] TableNames
		{
			get { return new string[] { JobDeclarationSchema.Constants.TableName }; }
		}

		public override string[] EventTypes
		{
			get { return new string[] { Events.DocumentSent.Code }; }  // DSN
		}

		public override bool IsRequired
		{
			get
			{
				return true;
			}
		}

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			foreach (var queuedLog in queuedLogs)
			{
				if (queuedLog.SJ_SE_NKEvent == Events.DocumentSent.Code && queuedLog.SJ_Reference.Contains(Core.Constants.RefDocTypeDescriptions.QuarantineRemotePrint))
				{
					var dec = queuedLog.Factory.Load<JobDeclaration>(queuedLog.SJ_ParentID);
					if (dec != null && !dec.IsNEXDOCSActive)
					{
						DeleteQuarantineRemotePrint(dec, queuedLog);
					}
				}
			}
		}

		void DeleteQuarantineRemotePrint(JobDeclaration dec, IQueuedLog queuedLog)
		{
			var documentToDelete = ExtractDocumentName(queuedLog.SJ_Reference);
			var dmi = dec.DocManagerInfo;
			if (!string.IsNullOrEmpty(documentToDelete) && dmi != null)
			{
				var docsToDelete = dmi.Files.Cast<IeDoc>().Where(x => x.DocType == Core.Constants.RefDocTypes.QuarantineRemotePrint && x.FileName.EqualsIgnoringCase(documentToDelete)).ToArray();
				docsToDelete.Cast<BusinessObject>().DeleteAll();
			}

			// Need this cos the declaration's factory and the declaration's doc manager's factory are not the same factory.  Need to wire them together. This effing line took an hour to crack, goddammit.
			dec.Factory.Saved += (BusinessObjectFactory factory, bool savedSuccessfully) =>
			{
				if (dec.Factory == dmi.MasterFactory.FactoryForEverythingExceptEDocs)
				{
					dmi.MasterFactory.SaveFactoriesExceptFactoryForEverythingExceptEDocs();
				}
				else
				{
					dmi.MasterFactory.Save();
				}
			};
		}

		protected string ExtractDocumentName(string referenceText)
		{
			var result = "";
			var logParameters = StmALog.GetParametersFromReference(referenceText);
			logParameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Name, out result);
			return result;
		}
	}
}
