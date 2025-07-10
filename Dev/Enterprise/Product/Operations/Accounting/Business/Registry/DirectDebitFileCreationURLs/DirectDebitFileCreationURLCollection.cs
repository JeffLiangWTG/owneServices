using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class DirectDebitFileCreationURLCollection : RegistryBusinessObjectCollectionTemplate
	{
		public DirectDebitFileCreationURLCollection()
		{
		}

		public DirectDebitFileCreationURLCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		public DirectDebitFileCreationURL Find(ZGuid bankAccountPK)
		{
			DirectDebitFileCreationURL result = null;

			foreach (DirectDebitFileCreationURL current in this)
			{
				if (current.BankAccountPK == bankAccountPK)
				{
					result = current;
					break;
				}
			}
			return result;
		}

		#region Overriden

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DirectDebitFileCreationURLCollection(fallbackLevel, CurrentFactory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DirectDebitFileCreationURL(CurrentFallbackLevel, CurrentFactory);
		}

		#endregion

		public new DirectDebitFileCreationURL this[int index]
		{
			get { return (DirectDebitFileCreationURL)Elements[index]; }
		}

		public new DirectDebitFileCreationURL AddNew()
		{
			return (DirectDebitFileCreationURL)base.AddNew();
		}
	}
}