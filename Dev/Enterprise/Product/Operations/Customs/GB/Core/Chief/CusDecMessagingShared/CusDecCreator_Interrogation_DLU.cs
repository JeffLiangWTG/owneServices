using System;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Edifact.D04A.Elements;
using Enterprise.Edifact.D04A.Messages.CUSDEC;

namespace Enterprise.Customs.GB.Chief.CusDec
{
	public class CusDecCreator_Interrogation_DLU : CusDecCreator
	{
		public CusDecCreator_Interrogation_DLU(QueryMessageFunction queryMessageFunction, ErrorCollector errorCollector) : base(queryMessageFunction, errorCollector)
		{
			this.queryMessageFunction = queryMessageFunction;
		}

		public CUSDECMessage CreateInterrogationMessage(string eoriNumber, string licenceNumber, string lineNumber, string nextDTM = "")
		{
			if (queryMessageFunction is Interrogate_DLU)
			{
				CreateInterrogationShellNoUnt();

				if (!string.IsNullOrEmpty(nextDTM))
				{
					var dtm = cusDec.DTM.InstantiateAChildAndAddItToChildrenCollection();
					dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.GetFromString("577");
					dtm.DateTimePeriod.DateOrTimeOrPeriodText = nextDTM;
					dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.GetFromString("203");
				}

				var rffEx = cusDec.Group1[0].RFF.InstantiateAChildAndAddItToChildrenCollection();
				rffEx.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.GetFromString(ChiefConstants.RffSegmentIdentifiers.EX);
				rffEx.Reference.ReferenceIdentifier = ChiefTextClass.T1(licenceNumber); //LI-REF
				rffEx.Reference.DocumentLineIdentifier = lineNumber; //LI-LINE-NO
				var rffAsm = cusDec.Group1[0].RFF.InstantiateAChildAndAddItToChildrenCollection();
				rffAsm.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.GetFromString(ChiefConstants.RffSegmentIdentifiers.ASM);
				rffAsm.Reference.ReferenceIdentifier = eoriNumber; //LI-TDR-ID

				FooterUNT();
				return cusDec;
			}

			throw new NotSupportedException("This is only for DLU");
		}
	}
}
