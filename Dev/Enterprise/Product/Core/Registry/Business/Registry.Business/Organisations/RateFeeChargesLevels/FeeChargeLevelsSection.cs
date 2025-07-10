using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class FeeChargeLevelsSection : RegistryBusinessObjectTemplate
	{
		#region Schema

		protected abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string FeeChargeTypes = "FeeChargeTypes";
		}

		#endregion

		#region FeeChargeTypes

		public FeeChargeTypeCollection FeeChargeTypes
		{
			get
			{
				if (feeChargeTypes == null)
				{
					feeChargeTypes = new FeeChargeTypeCollection();
					RegisterEditableChildObject(feeChargeTypes);
				}

				return feeChargeTypes;
			}
		}

		FeeChargeTypeCollection feeChargeTypes;

		public static FeeChargeLevelsSection GetDefault()
		{
			var result = new FeeChargeLevelsSection();
			result.FeeChargeTypes.AddFeeChargeType(ServiceTypesConstants.Code.DomesticWarranty, ServiceTypesConstants.Description.DomesticWarranty, FeeChargeLevelCollection.GetDefault());
			result.FeeChargeTypes.AddFeeChargeType(ServiceTypesConstants.Code.IntWarranty, ServiceTypesConstants.Description.IntWarranty, FeeChargeLevelCollection.GetDefault());
			result.FeeChargeTypes.AddFeeChargeType(ServiceTypesConstants.Code.FuelSurcharge, ServiceTypesConstants.Description.FuelSurcharge, FeeChargeLevelCollection.GetDefault());

			return result;
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new FeeChargeLevelsSection();

			foreach (FeeChargeType obj in FeeChargeTypes)
			{
				result.FeeChargeTypes.Add((FeeChargeType)obj.Clone(fallbackLevel, factory));
			}

			return result;
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			FeeChargeTypesSerializer.Serialize(writer, FeeChargeTypes);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			var types = (FeeChargeTypeCollection)FeeChargeTypesSerializer.Deserialize(reader);

			foreach (var obj in types)
			{
				FeeChargeTypes.Add(obj);
			}
		}

		ZXmlSerializer FeeChargeTypesSerializer
		{
			get { return feeChargeTypesSerializer ?? (feeChargeTypesSerializer = ZXmlSerializer.New(typeof(FeeChargeTypeCollection))); }
		}

		ZXmlSerializer feeChargeTypesSerializer;

		#region Constants

		static class ServiceTypesConstants
		{
			internal static class Code
			{
				internal const string DomesticWarranty = "DWY";
				internal const string IntWarranty = "IWY";
				internal const string FuelSurcharge = "FSE";
			}

			internal static class Description
			{
				internal static MultilingualString DomesticWarranty { get { return ResString.GetMultilingualString("83e86f60-0916-486a-be35-5efe3a88b315", "Domestic Warranty"); } }
				internal static MultilingualString IntWarranty { get { return ResString.GetMultilingualString("b17ec42a-30d3-4da8-9418-20765eb9ac23", "International Warranty"); } }
				internal static MultilingualString FuelSurcharge { get { return ResString.GetMultilingualString("98b52302-19b5-43fd-a3e6-443edbc1e3d4", "Fuel Surcharge"); } }
			}
		}

		#endregion
	}
}
