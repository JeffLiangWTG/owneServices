using System.Collections;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business;

public class CusAuthorizationUsageLookups : EU.Business.CusAuthorizationUsageLookups
{
	public CusAuthorizationUsageLookups(CusAuthorizationUsage parent) : base(parent)
	{
	}

	public new CusAuthorizationUsage Parent => (CusAuthorizationUsage)base.Parent;

	public override ICollection CodeList
	{
		get
		{
			var jobDeclaration = JobDeclaration;
			var messageType = jobDeclaration?.JE_MessageType;
			var parentIsInvoiceLine = Parent.InvoiceLine != null;

			return Factory.GetCachedValue(string.Join("|", "CusAuthorizationUsage.CodeList", messageType, parentIsInvoiceLine, jobDeclaration?.IsExport), () =>
			{
				var codeList = new CodeDescriptionPairList();

				if (!parentIsInvoiceLine)
				{
					codeList = (jobDeclaration?.IsExport ?? false) ? CodeListExport() : CodeListImport();
				}
				else
				{
					codeList = (jobDeclaration?.IsExport ?? false) ? CodeListExportItem() : CodeListImportItem();
				}

				codeList.Sort();
				return codeList;
			});
		}
	}

	CodeDescriptionPairList CodeListImport()
	{
		var result = CodeListImportExport();
		result.AddPair(NLCusAuthorisationHeaderTypeList.Codes.C526, NLCusAuthorisationHeaderTypeList.Descriptions.C526);
		result.AddPair(NLCusAuthorisationHeaderTypeList.Codes.C505, NLCusAuthorisationHeaderTypeList.Descriptions.C505);
		result.AddPair(NLCusAuthorisationHeaderTypeList.Codes.C504, NLCusAuthorisationHeaderTypeList.Descriptions.C504);
		result.AddPair(NLCusAuthorisationHeaderTypeList.Codes.C506, NLCusAuthorisationHeaderTypeList.Descriptions.C506);
		result.AddPair(NLCusAuthorisationHeaderTypeList.Codes.C516, NLCusAuthorisationHeaderTypeList.Descriptions.C516);
		return result;
	}

	CodeDescriptionPairList CodeListExport()
	{
		return CodeListImportExport();
	}

	CodeDescriptionPairList CodeListImportExport()
	{
		var result = new CodeDescriptionPairList();
		result.AddPair(NLCusAuthorisationHeaderTypeList.Codes.C501, NLCusAuthorisationHeaderTypeList.Descriptions.C501);
		result.AddPair(NLCusAuthorisationHeaderTypeList.Codes.C503, NLCusAuthorisationHeaderTypeList.Descriptions.C503);
		result.AddPair(NLCusAuthorisationHeaderTypeList.Codes.C502, NLCusAuthorisationHeaderTypeList.Descriptions.C502);
		result.AddPair(NLCusAuthorisationHeaderTypeList.Codes.C513, NLCusAuthorisationHeaderTypeList.Descriptions.C513);
		result.AddPair(NLCusAuthorisationHeaderTypeList.Codes.C519, NLCusAuthorisationHeaderTypeList.Descriptions.C519);
		result.AddPair(NLCusAuthorisationHeaderTypeList.Codes.C517, NLCusAuthorisationHeaderTypeList.Descriptions.C517);
		result.AddPair(NLCusAuthorisationHeaderTypeList.Codes.C514, NLCusAuthorisationHeaderTypeList.Descriptions.C514);
		result.AddPair(NLCusAuthorisationHeaderTypeList.Codes.C601, NLCusAuthorisationHeaderTypeList.Descriptions.C601);
		result.AddPair(NLCusAuthorisationHeaderTypeList.Codes.C019, NLCusAuthorisationHeaderTypeList.Descriptions.C019);
		result.AddPair(NLCusAuthorisationHeaderTypeList.Codes.C508, NLCusAuthorisationHeaderTypeList.Descriptions.C508);
		result.AddPair(NLCusAuthorisationHeaderTypeList.Codes.C507, NLCusAuthorisationHeaderTypeList.Descriptions.C507);
		result.AddPair(NLCusAuthorisationHeaderTypeList.Codes.C512, NLCusAuthorisationHeaderTypeList.Descriptions.C512);
		result.AddPair(NLCusAuthorisationHeaderTypeList.Codes.C509, NLCusAuthorisationHeaderTypeList.Descriptions.C509);
		return result;
	}

	CodeDescriptionPairList CodeListImportItem()
	{
		var result = CodeListImportExportItem();
		result.AddPair(NLCusAuthorisationHeaderTypeList.Codes.C990, NLCusAuthorisationHeaderTypeList.Descriptions.C990);
		result.AddPair(NLCusAuthorisationHeaderTypeList.Codes.D019, NLCusAuthorisationHeaderTypeList.Descriptions.D019);
		result.AddPair(NLCusAuthorisationHeaderTypeList.Codes.N990, NLCusAuthorisationHeaderTypeList.Descriptions.N990);
		return result;
	}

	CodeDescriptionPairList CodeListExportItem()
	{
		return CodeListImportExportItem();
	}

	CodeDescriptionPairList CodeListImportExportItem()
	{
		var result = new CodeDescriptionPairList();
		result.AddPair(NLCusAuthorisationHeaderTypeList.Codes.C626, NLCusAuthorisationHeaderTypeList.Descriptions.C626);
		result.AddPair(NLCusAuthorisationHeaderTypeList.Codes.C627, NLCusAuthorisationHeaderTypeList.Descriptions.C627);
		return result;
	}
}
