using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	public class CusAddInfoTypeListProvider : EU.DataTransfer.Universal.CusAddInfoTypeListProvider
	{
		protected override ICodeDescriptionPairList TableSpecificCusAddInfoTypeListCore(ZString tableCode, string dataContext)
		{
			ICodeDescriptionPairList result = null;
			switch (tableCode)
			{
				case JobDeclarationSchema.Constants.Prefix:
					result = GetListForJobDeclaration();
					break;
				case CusEntryHeaderSchema.Constants.Prefix:
					result = GetListForCusEntryHeader();
					break;
				default:
					result = base.TableSpecificCusAddInfoTypeListCore(tableCode, dataContext);
					break;
			}
			return result;
		}

		#region Implementation

		static CodeDescriptionPairList GetListForCusEntryHeader()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusAddInfoTypeAttribute.Codes.GBMaritimeUCNThatIsHeld, CusAddInfoTypeAttributeDescriptions.GBMaritimeUCNThatIsHeld);
			return result;
		}

		protected override CodeDescriptionPairList GetListForJobDeclaration()
		{
			var result = base.GetListForJobDeclaration();
			result.AddPair(CusAddInfoTypeAttribute.Codes.GBAllSimpleProperties, CusAddInfoTypeAttributeDescriptions.GBAllSimpleProperties);
			return result;
		}

		public new static class CusAddInfoTypeAttributeDescriptions
		{
			public static string GBMaritimeUCNThatIsHeld
			{
				get { return Res.GetString("8D3B20A1-7DF4-4C4D-81E6-55865B9CE176", "Maritime UCN That Is Held"); }
			}

			public static string GBAllSimpleProperties
			{
				get { return Res.GetString("49828C66-A042-41E6-A724-B5C0C46AA1D4", "All Simple Properties"); }
			}
		}

		#endregion
	}
}
