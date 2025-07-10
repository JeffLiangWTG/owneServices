using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class WebSecurityMappingLookups : ZLookups
	{
		public WebSecurityMappingLookups(WebSecurityMapping parent)
			: base(parent)
		{
		}

		public virtual CodeDescriptionPairList ModuleMappingList
		{
			get
			{
				if (Parent.ProductMapping.IsEmpty)
				{
					return new CodeDescriptionPairList();
				}

				return EDIDataRegistry.Instance.SystemProductMappings.Value.GetModuleList(Parent.ProductMapping);
			}
		}

		public virtual CodeDescriptionPairList ProductMappingList
		{
			get
			{
				return new ProductTypes();
			}
		}

		public virtual CodeDescriptionPairList WebSecurityList
		{
			get
			{
				var fullSecurityRights = new List<WebSecurityRight>();
				fullSecurityRights.AddRange(EDIWebSecurityRightsList.New());

				var result = new CodeDescriptionPairList();
				foreach (WebSecurityRight right in fullSecurityRights)
				{
					result.Add(new CodeDescriptionPair(right.Code, right.Description));
				}

				return result;
			}
		}

		#region Implementation

		protected new WebSecurityMapping Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (WebSecurityMapping)base.Parent; }
		}

		#endregion
	}
}

