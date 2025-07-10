using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Licencing.Business
{
	public sealed class ProductTypes : CodeDescriptionPairList
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "This is meant to specify an exact product name.")]
		public ProductTypes(bool includeCargoWiseOne = false, bool includeInternal = true)
		{
			if (includeCargoWiseOne)
			{
				AddPair(Codes.CargoWiseNext, Descriptions.CargoWiseNext);
				AddPair(Codes.CargoWise, Descriptions.CargoWise);
				AddPair(Codes.CargoWiseOne, Descriptions.CargoWiseOne);
				AddPair(Codes.Enterprise, Descriptions.Enterprise);
			}
			else
			{
				AddPair(Codes.Enterprise, Descriptions.CargoWise);
			}

			foreach (SystemProduct product in EDIDataRegistry.Instance.SystemProductMappings.Value)
			{
				if (product.Enabled && (includeInternal || !product.IsInternal))
				{
					AddPairIfNotExist(product.Code, product.Description);
				}
			}
			foreach (SystemProduct product in EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.Value)
			{
				if (product.Enabled && includeInternal)
				{
					AddPairIfNotExist(product.Code, product.Description);
				}
			}
			foreach (SystemProduct product in EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.Value)
			{
				if (product.Enabled && includeInternal)
				{
					AddPairIfNotExist(product.Code, product.Description);
				}
			}
		}

		public static bool IsEnterpriseFamily(string productCode)
		{
			return EnterpriseFamilyProducts.Contains(productCode);
		}

		public static IEnumerable<string> ProductsWithENTLicense
		{
			get
			{
				yield return Codes.Telematics;
			}
		}

		public static IEnumerable<string> EnterpriseFamilyProducts
		{
			get
			{
				return new[] { Codes.CargoWiseOne, Codes.CargoWiseNext, Codes.CargoWise, Codes.Enterprise, Codes.ProductivityWise };
			}
		}

		public static IEnumerable<string> CargoWiseNextAgreementCompatibleProducts
		{
			get
			{
				return new[] { Codes.CargoWiseOne, Codes.Enterprise, Codes.CargoWiseNext, Codes.CargoWise };
			}
		}

		public static class Codes
		{
			public const string Enterprise = "ENT";
			public const string CargoWiseOne = "CW1";
			public const string GLOW = "GLW";
			public const string CargoWiseNext = "CWN";
			public const string CargoWise = "CGW";

			//Will continue to be hardcoded for logic however they are now required to exist in registry
			public const string EHub = "HUB";
			public const string BorderWise = "BOR";
			public const string ProductivityWise = "PRW";
			public const string CargoSphere = "CSP";

			public const string WiseTechAcademy = "WTA";
			public const string Telematics = "TEL";
			public const string Sapphire = "SPH";

			public const string WTGInternal = "IST";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public static class Descriptions
		{
			public const string EnterpriseCW1 = "ediEnterprise / CargoWise One";
			public const string Enterprise = "ediEnterprise";
			public const string CargoWiseOne = "CargoWise One";
			public const string Glow = "GLOW ";
			public const string CargoWise = "CargoWise";
			public const string CargoWiseNext = "CargoWise Next";
		}
	}
}
