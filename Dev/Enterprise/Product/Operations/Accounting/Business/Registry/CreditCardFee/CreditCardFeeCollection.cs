using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class CreditCardFeeCollection : RegistryBusinessObjectCollectionTemplate
	{
		public CreditCardFeeCollection()
		{
		}

		public CreditCardFeeCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		#region Factory

		BusinessObjectFactory factory;
		BusinessObjectFactory LocalFactory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}

				return factory;
			}
		}

		#endregion

		#region Collection Implementation

		public new CreditCardFee this[int index]
		{
			get { return (CreditCardFee)Elements[index]; }
		}

		public new CreditCardFee AddNew()
		{
			return (CreditCardFee)base.AddNew();
		}

		public bool IsDuplicateItem(CreditCardFee itemToCheck)
		{
			bool result = false;

			foreach (CreditCardFee item in this)
			{
				if (item != itemToCheck && item.ChargeCodePK == itemToCheck.ChargeCodePK)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		public bool Contains(ZString code)
		{
			return FindByCode(code) != null;
		}

		public CreditCardFee FindByCode(ZString code)
		{
			ZGuid pK = LocalFactory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, code)).PK;

			foreach (CreditCardFee element in this)
			{
				if (element.ChargeCodePK == pK)
				{
					return element;
				}
			}

			return null;
		}

		#endregion

		#region Overriden

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CreditCardFeeCollection(fallbackLevel);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CreditCardFee(CurrentFallbackLevel);
		}

		#endregion
	}
}
