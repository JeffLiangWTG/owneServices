using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class DSBJobManagement : JobManagement
	{
		public DSBJobManagement(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return true;
		}

		#region DSBSurplusAndShortfallAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal DSBSurplusAndShortfallAmount { set; get; }

		#endregion
	}
}
