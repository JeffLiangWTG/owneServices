using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class JXCWarningInfoCollector : IEnumerable<JXCWarningInfo>
	{
		public JXCWarningInfoCollector(IBusiness businessEntity)
		{
			this.BusinessEntity = businessEntity;
			if (businessEntity == null)
			{
				throw new ArgumentNullException(nameof(businessEntity));
			}
		}

		public IEnumerator<JXCWarningInfo> GetEnumerator()
		{
			return GetElements().GetEnumerator();
		}

		IEnumerable<JXCWarningInfo> GetElements()
		{
			BusinessObject bizO = BusinessEntity as BusinessObject;
			if (bizO != null)
			{
				foreach (ZPropertyInfo info in bizO.ZPropertyInfoHash)
				{
					ZWrappedPropertyInfo wrappedInfo = info as ZWrappedPropertyInfo;
					if (wrappedInfo != null &&
						wrappedInfo.InnerInfo != null &&
						(((IList<IBusiness>)BusinessEntity.Children).Contains(wrappedInfo.InnerInfo.BizObj) ||
						 wrappedInfo.InnerInfo.BizObj == bizO))
					{
						continue;
					}

					foreach (string warning in info.GetWarnings().GetUniqueMessageList())
					{
						if (warning.StartsWith(JXCConstants.JXCWarningPrefix))
						{
							yield return new JXCWarningInfo(info, warning.Remove(0, JXCConstants.JXCWarningPrefix.Length));
						}
					}
				}

				foreach (IBusiness child in BusinessEntity.Children)
				{
					foreach (JXCWarningInfo jXCWarningInfo in new JXCWarningInfoCollector(child))
					{
						yield return jXCWarningInfo;
					}
				}
			}
			else
			{
				BusinessObjectCollection collection = BusinessEntity as BusinessObjectCollection;
				if (collection != null)
				{
					foreach (IBusiness childBizO in collection)
					{
						foreach (JXCWarningInfo jXCWarningInfo in new JXCWarningInfoCollector(childBizO))
						{
							yield return jXCWarningInfo;
						}
					}
				}
			}
		}

		public bool IsEmpty
		{
			get { return !GetElements().Any(); }
		}

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		readonly IBusiness BusinessEntity;
	}
}
