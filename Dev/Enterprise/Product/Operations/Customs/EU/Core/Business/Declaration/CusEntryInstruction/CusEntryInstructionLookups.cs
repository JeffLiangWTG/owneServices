using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Univ = Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusEntryInstructionLookups : Customs.Business.CusEntryInstructionLookups
	{
		public CusEntryInstructionLookups(CusEntryInstruction cusEntryInstruction)
			: base(cusEntryInstruction)
		{
		}

		public new CusEntryInstruction Parent
		{
			get { return (CusEntryInstruction)base.Parent; }
		}

		public override CodeDescriptionPairList EntrySubStyleList
		{
			get { return GetEntrySubstyleList(Parent?.JobDeclaration); }
		}

		protected virtual CodeDescriptionPairList GetEntrySubstyleList(ICanBeImportOrExport parent)
		{
			var key = "EU.CusEntry.Lookups.EntrySubStyleList";
			if (parent != null)
			{
				key = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}", key, parent.DataGroupingCode, (parent.IsImport ? "IMP" : "EXP"));
			}

			return base.Factory.GetCachedValue(key, delegate
			{
				var result = new CodeDescriptionPairList();
				if (parent != null && (parent.IsImport || parent.IsExport))
				{
					result = Factory.GetCachedValue(key, () => ReConstructCodeDescriptionPairList(parent, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensub, Factory.GetCachedValue<EntrySubStyleList>()));
				}
				return result;
			});
		}

		CodeDescriptionPairList ReConstructCodeDescriptionPairList(ICanBeImportOrExport parent, string code, CodeDescriptionPairList codeDescriptionPairList)
		{
			var result = new CodeDescriptionPairList();
			var tempCodePairList = Univ.RefCusCodeListTypes.GetCachedList(Factory, parent.DataGroupingCode, code, ZDateTime.Now);

			foreach (CodeDescriptionPair codeDescriptionPair in codeDescriptionPairList)
			{
				var index = tempCodePairList.IndexOfCode(codeDescriptionPair.Code);
				result.Add(index >= 0 ? tempCodePairList[index] : codeDescriptionPair);
			}
			return result;
		}

		public override CodeDescriptionPairList StyleList
		{
			get { return DeclarationTypeList; }
		}

		public CodeDescriptionPairList DeclarationTypeList
		{
			get { return DeclarationTypeListCore; }
		}

		public CodeDescriptionPairList ProcedureCodeList => ProcedureCodeListCore;

		protected virtual CodeDescriptionPairList ProcedureCodeListCore
		{
			get
			{
				if (Parent is CusEntryInstruction instruction && instruction.JobDeclaration is JobDeclaration declaration && declaration.IsRequestedProcedureEnable)
				{
					var dataGroupingCode = declaration.GetDefaultDataGroupingCode();
					var messageType = declaration.JE_MessageType;
					var declarationType = instruction.CEI_Style;
					return RequestedProcedureHelper.GetCachedRequestedProcedureCodeList(Factory, dataGroupingCode, messageType, declarationType);
				}
				else
				{
					return new CodeDescriptionPairList();
				}
			}
		}

		protected virtual CodeDescriptionPairList DeclarationTypeListCore
		{
			get
			{
				var declaration = Parent?.JobDeclaration;
				var key = GetDeclarationTypeListCacheKey(declaration);
				return base.Factory.GetCachedValue(key, delegate
				{
					var result = new CodeDescriptionPairList();

					if (declaration != null)
					{
						var tempResult = new CodeDescriptionPairList();
						var groupCodesForMessageType = GetDeclarationTypeCodes(declaration);
						var descriptions = GetDefinedDeclarationTypeList(declaration);

						if (groupCodesForMessageType != null && groupCodesForMessageType.Any())
						{
							bool hasDefinedDescriptions = false;
							foreach (var code in groupCodesForMessageType)
							{
								string description = descriptions.GetDescriptionFromCode(code);
								if (!string.IsNullOrEmpty(description))
								{
									hasDefinedDescriptions = true;
									result.AddPairIfNotExist(code, description);
								}
								else
								{
									tempResult.AddPairIfNotExist(code, code.ToString());
								}
							}

							if (!hasDefinedDescriptions)
							{
								result = tempResult;
							}
							result.Sort();
						}
					}
					return result;
				});
			}
		}

		protected virtual string GetDeclarationTypeListCacheKey(JobDeclaration declaration)
		{
			var key = "JobDeclaration.Lookups.DeclarationTypeList";
			if (declaration != null)
			{
				key = string.Join("_", key, declaration.JE_MessageType, declaration.CountryCode, declaration.JE_ApplicationCode);
			}
			return key;
		}

		protected virtual IEnumerable<ZString> GetDeclarationTypeCodes(JobDeclaration declaration)
		{
			return new RefCusProcedure.Loader(Factory).LoadDistinctGroupCodes(declaration.JE_MessageType, declaration.GetDefaultDataGroupingCode());
		}

		protected virtual CodeDescriptionPairList GetDefinedDeclarationTypeList(JobDeclaration declaration)
		{
			return new CodeDescriptionPairList();
		}
	}
}
