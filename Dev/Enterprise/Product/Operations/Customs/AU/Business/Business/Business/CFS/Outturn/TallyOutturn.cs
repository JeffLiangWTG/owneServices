using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[DependentBusinessObject(typeof(TallyOutturnHeader), "Outturns")]
	public class TallyOutturn : DepotCusOutturn
	{
		public TallyOutturn(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Container

		public new TallyContainer Container
		{
			get { return (TallyContainer)base.Container; }
		}

		protected override CFSContainer GetContainerCore()
		{
			TallyContainer result = null;
			CFSTallyContainerWrapper wrapper = Parent as CFSTallyContainerWrapper;
			if (wrapper != null)
			{
				result = wrapper.Container;
			}

			return result;
		}

		#endregion

		#region Header

		public new TallyOutturnHeader Header
		{
			get { return (TallyOutturnHeader)base.Header; }
		}

		protected override CusOutturnHeader GetHeaderCore()
		{
			return Factory.Load<TallyOutturnHeader>(C5_C6);
		}

		#endregion

		#region Get Parent Loaders

		protected override TypeLoaderCollection GetParentLoaders()
		{
			TypeLoaderCollection result = new TypeLoaderCollection();
			result.Add(new WrapperTypeLoader<TallyContainer, CFSTallyContainerWrapper>());
			result.Add(new WrapperTypeLoader<PackUnpackShipment, CFSShipmentWrapper>());
			return result;
		}

		#endregion
		public bool C5_CargoUnpackDate_ReadOnly
		{
			get { return IsSynchronisedWithPackUnpachShipment; }
		}

		public bool C5_DamageIndicator_ReadOnly
		{
			get { return IsSynchronisedWithPackUnpachShipment; }
		}

		public bool C5_PillageIndicator_ReadOnly
		{
			get { return IsSynchronisedWithPackUnpachShipment; }
		}

		public bool C5_PackagesUnits_ReadOnly
		{
			get { return IsSynchronisedWithPackUnpachShipment; }
		}

		public bool C5_PackagesOutturned_ReadOnly
		{
			get { return IsSynchronisedWithPackUnpachShipment; }
		}

		public bool C5_CargoReceiptDate_ReadOnly
		{
			get { return IsSynchronisedWithTallyContainer; }
		}

		internal bool IsSynchronisedWithPackUnpachShipment { get; set; }
		internal bool IsSynchronisedWithTallyContainer { get; set; }
	}
}
