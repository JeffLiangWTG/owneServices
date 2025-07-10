using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirCTOExportCustomsManifestHeader : ExportCustomsManifestHeader
		, Customs.Business.IMessageManageableBizObj
		, IDocumentSupportable
	{
		public AirCTOExportCustomsManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable(true)]
		public new ExportCustomsManifestLinesCollection Lines
		{
			get { return base.Lines; }
		}

		#region BusinessObject Overrides

		protected override ZString HumanReadableNameCore => Res.GetString("DF88F491-D580-4102-B4BC-B7C80152F59E", "Air CTO - Export {0}", ED_BGMReference);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			ED_TransportMode = Core.Constants.TransportModes.Air;
			ED_ManifestType = AirManifestTypeList.Codes.ExportMainManifest;
		}

		protected override Customs.Business.ExportCustomsManifestHeaderLookups GetNewLookups()
		{
			return new AirCTOExportCustomsManifestHeaderLookups(this);
		}

		#endregion

		#region Overrides

		public override bool HaveMessagesBeenSentToCustoms
		{
			get
			{
				bool result = base.HaveMessagesBeenSentToCustoms;

				if (!result)
				{
					foreach (ExportCustomsManifestLines line in Lines)
					{
						if (line.Messages.Count > 0)
						{
							result = true;
							break;
						}
					}
				}

				return result;
			}
		}

		protected override bool TransportModeReadOnly
		{
			get { return true; }
		}

		protected override bool IsAirCTOHeaderCore
		{
			get { return true; }
		}

		#endregion

		#region IMessageManageableBizObj Members

		Customs.Business.IMessageManager Customs.Business.IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
		{
			return new AirCTOExportMessageManager(this);
		}

		bool Customs.Business.IMessageManageableBizObj.IsInAStatusAmendmentSendable
		{
			get { return true; }
		}

		Customs.Business.ContinueWithDetection Customs.Business.IMessageManageableBizObj.ProcessBeforeDetectingAmendmentAndContinue()
		{
			return Customs.Business.ContinueWithDetection.Yes;
		}

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return new AirCTOExportCustomsManifestHeaderDocumentSupporter(this); }
		}

		#endregion
	}
}
