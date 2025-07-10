using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class WebSecurityMappingCollection : RegistryBusinessObjectCollectionTemplate<WebSecurityMapping>
	{
		public WebSecurityMappingCollection()
			: base(null, null)
		{
		}

		public WebSecurityMapping AddNew(string webSecurity, string productMapping, string moduleMapping)
		{
			var mapping = AddNew();
			mapping.WebSecurity = webSecurity;
			mapping.ProductMapping = productMapping;
			mapping.ModuleMapping = moduleMapping;

			return mapping;
		}

		#region Validation

		protected override bool RunPreSaveValidationCore()
		{
			ValidateUnique();

			return base.RunPreSaveValidationCore();
		}

		void ValidateUnique()
		{
			foreach (var mapping in this)
			{
				mapping.ClearRowNotifications();
			}

			if (Count < 2)
			{
				return;
			}

			for (int i = 0; i < Count - 1; i++)
			{
				for (int j = i + 1; j < Count; j++)
				{
					if (this[i].WebSecurity == this[j].WebSecurity && this[i].ProductMapping == this[j].ProductMapping && this[i].ModuleMapping == this[j].ModuleMapping)
					{
						var error = Res.GetString("E3B36771-D634-4D26-ABE7-1280AC50510E", "The combination of Web Security, Product and Module must be unique");
						this[i].AddRowError(error);
						this[j].AddRowError(error);
					}
				}
			}
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new WebSecurityMappingCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new WebSecurityMapping();
		}

		public bool HasProduct(string product)
		{
			foreach (var mapping in this)
			{
				if (mapping.ProductMapping == product)
				{
					return true;
				}
			}

			return false;
		}

		#endregion
	}
}

