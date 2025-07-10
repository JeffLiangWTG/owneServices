using System.Collections;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRIDLMessage : CMRCUSRESMessage
	{
		public CMRIDLMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString SendersReferenceCore
		{
			get
			{
				return GetReference(CUSRES.Group6, ReferenceFunctionCodeQualifierList.OriginatorsReference);
			}
		}

		public override ZString StatusOfLinesReport
		{
			get
			{
				return ZString.Empty;
			}
		}

		protected internal override ArrayList GetErrorsArrayList()
		{
			ArrayList errorList = new ArrayList();
			foreach (SegmentGroup6 group6 in CUSRES.Group6)
			{
				foreach (FTXSegment fTX in group6.FTX)
				{
					errorList.Add(fTX.TextLiteral.FreeTextValue1);
				}
			}
			return errorList;
		}

		protected override ZString GetStatusCore()
		{
			return "IDLE";
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.IDL;
		}

		protected override internal BusinessObject GetWrappedObject()
		{
			ZString reference = GetReferenceFromSendersReference();
			return JobDeclaration.LoadFirstMatchingInCurrentCompanyIncludingInActive(Factory, reference)
				?? ExportCustomsManifestHeader.Load(Factory, reference)
				?? base.GetWrappedObject();
		}
	}
}
