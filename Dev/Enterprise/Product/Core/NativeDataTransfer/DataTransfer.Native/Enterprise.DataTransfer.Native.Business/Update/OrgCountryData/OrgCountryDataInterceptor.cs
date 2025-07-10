using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Interceptors;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgCountryData
{
	class OrgCountryDataInterceptor
		: BaseInterceptor
	{
		public OrgCountryDataInterceptor(IInterceptorSetting setting, AncillaryImportServices sessionServices)
			: base(setting, sessionServices)
		{
		}

		public override void Invoke(IEntitySet entitySet)
		{
			var orgHeader = entitySet.Root;
			CheckForDuffAddInfo(orgHeader);
			IgnoreEUClientCountryRelation(orgHeader);
			Function(entitySet);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]  // Need to catch general types because that is the point of this method 
		void CheckForDuffAddInfo(IEntity entity)
		{
			foreach (IEntity ocd in entity.Children.Where(c => c.EntityName == "OrgCountryData"))
			{
				foreach (var addInfo in ocd.Properties.Where(p => p.Name == "ImportCustomsDefaultAddInfo" || p.Name == "CustomsEconomicGroupAddInfo"))
				{
					if (addInfo != null && addInfo.Value != null && !String.IsNullOrEmpty(addInfo.Value.ToString()))
					{
						try
						{
							XElement.Parse(addInfo.Value.ToString());
						}
						catch (Exception e) when (!e.IsCriticalException())
						{
							addInfo.Value = ""; // If it's not XML, do not import anything in that field
						}
					}
				}
			}
		}

		void IgnoreEUClientCountryRelation(IEntity entity)
		{
			foreach (IEntity ocd in entity.Children.Where(c => c.EntityName == "OrgCountryData"))
			{
				foreach (IEntity clientCountryRelation in ocd.Parents.Where(p => p.EntityName == "ClientCountryRelation"))
				{
					if (clientCountryRelation.Properties.FirstOrDefault(x => x.Name == "Code" && x.Value.ToString() == "EU") != null)
					{
						clientCountryRelation.Action = EntityAction.IGNORE;
					}
				}
			}
		}
	}
}
