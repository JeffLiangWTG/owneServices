using System.Linq;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	class JTTChinesePSNComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => true;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			var substance = wrapper?
				.DGData?
				.Substance?
				.StandardSubstance as UNDGSubstanceJTT;

			if (substance != null)
			{
				if (ShouldWriteProperShippingNameInChinese())
				{
					return substance
						.Names
						.FirstOrDefault(name => name.DAZ_Type == ViewUNDGAttributeLookups.TypeConstants.ProperShippingName)
						?.DAZ_Descriptor ?? ZString.Empty;
				}
				else
				{
					return substance.JTT_PSN;
				}
			}

			return ZString.Empty;
		}

		bool ShouldWriteProperShippingNameInChinese()
		{
			return Res.CurrentLanguage == Core.Constants.Languages.ChineseSimplified
				|| (GlbStaff.CurrentUser?.Language ?? ZString.Empty) == Core.Constants.Languages.ChineseSimplified;
		}
	}
}
