using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business.Declaration;

public class CusEntryInstructionLookups : EU.Business.Declaration.CusEntryInstructionLookups
{
	public CusEntryInstructionLookups(CusEntryInstruction cusEntryInstruction)
		: base(cusEntryInstruction)
	{
	}

	public new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

	public override CodeDescriptionPairList EntrySubStyleList
	{
		get
		{
			var list = new CodeDescriptionPairList();
			if (Parent.JobDeclaration is JobDeclaration declaration)
			{
				list = Factory.GetCachedValue(string.Join("|", "NL.CusEntryInstructionsLookups.EntrySubStyleList", declaration.JE_MessageType), () =>
				{
					list = new DeclarationSubTypeList();
					if (declaration.IsImport)
					{
						list.RemoveCode(DeclarationSubTypeList.Codes.R);
					}
					return list;
				});
			}
			return list;
		}
	}

	public CodeDescriptionPairList CPCList
	{
		get
		{
			var result = new CodeDescriptionPairList();
			if (Parent.JobDeclaration is JobDeclaration jobDeclaration)
			{
				var dataGroupingCode = jobDeclaration.GetDefaultDataGroupingCode();
				var dateOfValuation = jobDeclaration.DateOfValuation;
				var style = Parent.CEI_Style;
				var messageType = jobDeclaration.JE_MessageType;
				result = Factory.GetCachedValue(string.Join("|", "NL.CusEntryInstructionLookups.CPCList", string.Join("_", dataGroupingCode, dateOfValuation, style, messageType)), () =>
				{
					var list = new CodeDescriptionPairList();
					new RefCusProcedureCollection(Factory, dataGroupingCode, dateOfValuation, style, messageType).Cast<RefCusProcedure>().ForEach(x => list.AddPairIfNotExist(x.ZZ6_ProcedureCode, x.ZZ6_Description));
					return list;
				});
			}
			return result;
		}
	}

	protected override CodeDescriptionPairList GetDefinedDeclarationTypeList(EU.Business.Declaration.JobDeclaration declaration)
	{
		var result = new DeclarationTypeList();

		switch (declaration.JE_MessageType)
		{
			case MessageTypeList.Codes.Import:
				result.RemoveCode(Business.DeclarationTypeList.Codes.B1);
				result.RemoveCode(Business.DeclarationTypeList.Codes.B2);
				result.RemoveCode(Business.DeclarationTypeList.Codes.B3);
				result.RemoveCode(Business.DeclarationTypeList.Codes.B4);
				result.RemoveCode(Business.DeclarationTypeList.Codes.C1);
				result.RemoveCode(Business.DeclarationTypeList.Codes.C2);
				break;
			case MessageTypeList.Codes.Export:
				result.RemoveCode(Business.DeclarationTypeList.Codes.H1);
				result.RemoveCode(Business.DeclarationTypeList.Codes.H2);
				result.RemoveCode(Business.DeclarationTypeList.Codes.H3);
				result.RemoveCode(Business.DeclarationTypeList.Codes.H4);
				result.RemoveCode(Business.DeclarationTypeList.Codes.H5);
				result.RemoveCode(Business.DeclarationTypeList.Codes.H6);
				result.RemoveCode(Business.DeclarationTypeList.Codes.I1);
				result.RemoveCode(Business.DeclarationTypeList.Codes.I2);
				break;
		}

		return result;
	}
}
