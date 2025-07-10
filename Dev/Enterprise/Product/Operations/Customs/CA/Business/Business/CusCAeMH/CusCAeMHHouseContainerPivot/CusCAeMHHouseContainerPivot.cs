using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuilders;

namespace Enterprise.Customs.CA.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class CusCAeMHHouseContainerPivot : AutoCusCAeMHHouseContainerPivot,
		IHouseBillContainer,
		Customs.Business.ISynchroniserReadOnlyMembersProvider, Integration.Customs.CA.ICusCAeMHHouseContainerPivot
	{
		public CusCAeMHHouseContainerPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region New properties

		public ZString BPA_Calc_ContainerNumber
		{
			get
			{
				var container = Container;
				return container != null ? container.BQ_ContainerNumber : ZString.Empty;
			}
		}

		#endregion

		#region Overriden

		[List(nameof(Lookups) + "." + nameof(CusCAeMHHouseContainerPivotLookups.Containers))]
		[RelatedBusinessObject("Container")]
		public override ZGuid BPA_BQ_Container
		{
			get { return base.BPA_BQ_Container; }
			set { base.BPA_BQ_Container = value; }
		}

		public CusCAeMHContainer Container
		{
			get { return Factory.Load<CusCAeMHContainer>(this.BPA_BQ_Container); }
		}

		[RelatedBusinessObject("HouseBill")]
		public override ZGuid BPA_BW_House
		{
			get { return base.BPA_BW_House; }
			set { base.BPA_BW_House = value; }
		}

		public CusCAeMHHouse HouseBill
		{
			get { return Factory.Load<CusCAeMHHouse>(this.BPA_BW_House); }
		}

		public CusCAeMHMaster MasterBill
		{
			get { return HouseBill.MasterBill; }
		}

		#endregion

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion

		#region IHouseBillContainer

		ZString IHouseBillContainer.ContainerNumber
		{
			get { return Container == null ? ZString.Empty : Container.BQ_ContainerNumber; }
		}

		IEnumerable<ZString> IHouseBillContainer.Seals
		{
			get { return Container == null ? System.Array.Empty<ZString>() : new ZString[] { Container.BQ_Seal1, Container.BQ_Seal2 }; }
		}

		#endregion
	}
}
