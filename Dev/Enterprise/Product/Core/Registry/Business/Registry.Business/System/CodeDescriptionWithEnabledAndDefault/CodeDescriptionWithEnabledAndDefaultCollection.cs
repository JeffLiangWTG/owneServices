using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CodeDescriptionWithEnabledAndDefaultCollection : RegistryBusinessObjectCollection
	{
		public CodeDescriptionWithEnabledAndDefaultCollection()
			: this(0)
		{
		}

		public CodeDescriptionWithEnabledAndDefaultCollection(int codeMaxLength)
			: base(null, codeMaxLength)
		{
		}

		#region Properties

		public new CodeDescriptionWithEnabledAndDefault this[int i]
		{
			get { return (CodeDescriptionWithEnabledAndDefault)base[i]; }
		}

		public CodeDescriptionWithEnabledAndDefault Default
		{
			get
			{
				foreach (CodeDescriptionWithEnabledAndDefault item in this)
				{
					if (item.IsDefault)
					{
						return item;
					}
				}

				return null;
			}
		}

		public IEnumerable<CodeDescriptionWithEnabledAndDefault> GetEnabled()
		{
			return this.Cast<CodeDescriptionWithEnabledAndDefault>().Where(item => item.IsEnabled);
		}

		#endregion

		#region AddNew

		public new CodeDescriptionWithEnabledAndDefault AddNew()
		{
			return (CodeDescriptionWithEnabledAndDefault)base.AddNew();
		}

		public CodeDescriptionWithEnabledAndDefault AddNew(ZString code, MultilingualString description, bool isDefault, bool isEnabled)
		{
			var result = AddNew();
			using (result.GetValidationSuspender())
			{
				result.Code = code;
				result.Description = description;
				result.IsDefault = isDefault;
				result.IsEnabled = isEnabled;
			}

			return result;
		}

		public CodeDescriptionWithEnabledAndDefault AddNewSystemDefined(ZString code, MultilingualString description, bool isDefault, bool isEnabled)
		{
			var result = AddNew(code, description, isDefault, isEnabled);
			result.IsSystemDefined = true;
			return result;
		}

		#endregion

		#region Get Active CodeDescriptionPairList

		public CodeDescriptionPairList GetActiveCodeDescriptionPairList()
		{
			var result = new CodeDescriptionPairList();
			foreach (CodeDescriptionWithEnabledAndDefault element in this)
			{
				if (element.IsEnabled)
				{
					result.AddPair(element.Code, element.Description);
				}
			}
			return result;
		}

		#endregion

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CodeDescriptionWithEnabledAndDefault();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CodeDescriptionWithEnabledAndDefaultCollection(CodeMaxLength);
		}

		#endregion
	}
}
