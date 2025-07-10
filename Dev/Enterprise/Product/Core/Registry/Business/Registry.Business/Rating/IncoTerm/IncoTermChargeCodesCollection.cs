using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;
using Incoterms = Enterprise.Core.Constants.IncoTerms;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class IncoTermChargeCodesCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new IncoTermChargeCodes this[int i]
		{
			get { return (IncoTermChargeCodes)Elements[i]; }
		}

		public new IncoTermChargeCodes AddNew()
		{
			return (IncoTermChargeCodes)base.AddNew();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new IncoTermChargeCodesCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new IncoTermChargeCodes();
		}

		[ThreadSafe]
		static IncoTermChargeCodesCollection defaultIncoTermChargeCodes = null;

		[ThreadSafe]
		static readonly object lockobject = new object();

		public static IncoTermChargeCodesCollection GetDefault()
		{
			if (defaultIncoTermChargeCodes == null)
			{
				lock (lockobject)
				{
					if (defaultIncoTermChargeCodes == null)
					{
						var incoTermChargeCodes = new IncoTermChargeCodesCollection();

						var activeIncoterms = Incoterms.Incoterms2000.Union(Incoterms.Incoterms2010).Union(Incoterms.Incoterms2020).ToList();
						activeIncoterms = activeIncoterms.Union(Incoterms.Incoterms2020).ToList();

						activeIncoterms.Sort();
						activeIncoterms.ForEach(code => incoTermChargeCodes.AddNew().SetDefaults(code));

						defaultIncoTermChargeCodes = incoTermChargeCodes;
					}
				}
			}
			return defaultIncoTermChargeCodes;
		}

#if DEBUG
		public static void ClearDefaultIncoTermChargeCodesForTesting()
		{
			defaultIncoTermChargeCodes = null;
		}
#endif
	}
}
