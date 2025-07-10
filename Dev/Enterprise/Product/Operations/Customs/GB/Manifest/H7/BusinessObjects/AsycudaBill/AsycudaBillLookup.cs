using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.H7.Business
{
	public class AsycudaBillLookups : EU.H7.Business.AsycudaBillLookups
	{
		public AsycudaBillLookups(AsycudaBill parent)
			: base(parent)
		{
		}

		public new AsycudaBill Parent => (AsycudaBill)base.Parent;

		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<Common.Shared.MessageStatusList>();

		public override CodeDescriptionPairList CustomsStatusList => GetCustomsStatusList(Factory);

		public static CodeDescriptionPairList GetCustomsStatusList(BusinessObjectFactory factory) => (CodeDescriptionPairList)Common.EntryStatusListHelper.EntryStatusList(factory, "GBH7", string.Empty);

		public CodeDescriptionPairList SubStyleList
		{
			get
			{
				var codes = new List<string>();
				if (Parent.Header.AMA_RL_NKPortOfDischarge.IsEmpty)
				{
					codes =
					[
						EntrySubStyleCodeList.Codes.A,
						EntrySubStyleCodeList.Codes.D,
						EntrySubStyleCodeList.Codes.J,
						EntrySubStyleCodeList.Codes.K,
					];
				}
				else if (Parent.Header.IsForBIRDS)
				{
					codes = [EntrySubStyleCodeList.Codes.J, EntrySubStyleCodeList.Codes.K];
				}
				else
				{
					codes = [EntrySubStyleCodeList.Codes.A, EntrySubStyleCodeList.Codes.D];
				}

				var codesKey = string.Concat(codes);

				return Factory.GetCachedValue($"GBH7AsycudaBillLookups|SubStyleList{codesKey}", delegate
				{
					var entryList = new EntrySubStyleCodeList(Factory);
					if (!entryList.IsLoaded)
					{
						entryList.Load();
					}

					var newList = new CodeDescriptionPairList();
					foreach (var code in codes)
					{
						var desc = entryList.GetDescriptionFromCode(code);
						newList.AddPair(code, desc);
					}
					return newList;
				});
			}
		}

		public override CodeDescriptionPairList AdditionalProcedureList
		{
			get
			{
				return Factory.GetCachedValue("GBH7AsycudaBillLookups|AdditionalProcedureList", delegate
				{
					var selectableAdditionalProcedureCodes = new List<ZString>() { "C07", "C08", "1RV", "F48", "F47" };

					var refCusCollection = RefCusProcedureCollection.LoadConcessionsForCountryShipmentTypeProcedureCodePreviousProceduresCode(Factory, "CDS", ZString.Empty, "40", "00", ZDateTime.Empty);
					var additionalProcedureList = refCusCollection.Where(x => selectableAdditionalProcedureCodes.Contains(x.ZZ6_Concession)).ToList();

					var additionalProcedureCodes = new CodeDescriptionPairList();
					foreach (var additionalProcedure in additionalProcedureList)
					{
						additionalProcedureCodes.AddPair(additionalProcedure.FullCodeCurrentPlusPreviousPlusConcession, additionalProcedure.ZZ6_Description);
					}

					return additionalProcedureCodes;
				});
			}
		}
	}
}
