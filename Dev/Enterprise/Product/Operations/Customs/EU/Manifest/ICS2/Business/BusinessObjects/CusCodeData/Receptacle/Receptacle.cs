using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class Receptacle : CusCodeDataWithOrder, IShortSequenceNumberLine
	{
		public Receptacle(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static string CodeDataType => CusCodeDataTypeList.Codes.EUICS2Receptacle;

		public new class Loader : CusCodeDataWithOrder.Loader
		{
			public Loader(BusinessObjectFactory factory) : base(factory)
			{
			}

			public Receptacle Load<TParent>(TParent parent, ZShort order)
				where TParent : BusinessObject
			{
				return Load<Receptacle, TParent>(parent, order, CodeDataType);
			}

			public Receptacle LoadOrCreate<TParent>(TParent parent, short order)
				where TParent : BusinessObject
			{
				return LoadOrCreate<Receptacle, TParent>(parent, order, CodeDataType);
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(Receptacle);
		}

		protected override ZString HumanReadableNameCore => Res.GetString("0AF36E2F-7858-40D4-ADFF-DCA2120B1ECC", "Receptacle");

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(AsycudaManifestHeader), typeof(AsycudaBill));

		public new ReceptacleValidation Validation => (ReceptacleValidation)base.Validation;

		protected override CusCodeDataValidation GetNewValidation() => new ReceptacleValidation(this);

		protected override string CusCodeDataType => CodeDataType;

		#region Properties

		[MaxLength(35)]
		[ResourceStringData("1B3E0FF3-7753-49DF-822C-054E6611B02C", Caption = "Receptacle ID(s)")]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set
			{
				var oldValue = CY_Data;
				base.CY_Data = value;
				if (oldValue != CY_Data)
				{
					CheckMaximumLength(CY_DataInfo, value);
					var parent = Parent;
					if (parent != null)
					{
						if (CY_ParentTableCode == AsycudaManifestHeaderSchema.Constants.Prefix)
						{
							((AsycudaManifestHeader)parent).ReceptacleIdInfo.RefreshBinding();
						}
						else if (CY_ParentTableCode == AsycudaBillSchema.Constants.Prefix)
						{
							((AsycudaBill)parent).ReceptacleIdInfo.RefreshBinding();
						}
					}
				}
			}
		}

		public override ZShort CY_Order
		{
			get => base.CY_Order;
			set
			{
				var oldValue = CY_Order;
				base.CY_Order = ShortSequenceNumberGenerator.EnsureValidSequenceNumber(value);
				if (!IsCopying)
				{
					if (CY_ParentTableCode == AsycudaManifestHeaderSchema.Constants.Prefix)
					{
						((AsycudaManifestHeader)Parent).Receptacles.SequenceNumberCalculator.RecalculateWhenRenumbered(this, oldValue);
					}
				}
			}
		}

		#endregion

		#region IShortSequenceNumberLine

		public ZShort SequenceNumber { get => CY_Order; set => CY_Order = value; }

		public ZGuid FKToHeader => CY_ParentID;

		public override ZGuid CY_ParentID
		{
			get => base.CY_ParentID;
			set
			{
				var oldValue = CY_ParentID;
				base.CY_ParentID = value;
				if (!IsCopying && oldValue != CY_ParentID && !CY_ParentID.IsValid)
				{
					DetachedFromParent(oldValue);
				}
			}
		}

		public override ZString CY_ParentTableCode
		{
			get => base.CY_ParentTableCode;
			set
			{
				var oldValue = CY_ParentTableCode;
				base.CY_ParentTableCode = value;
				if (!IsCopying && oldValue != CY_ParentTableCode && !CY_ParentTableCode.IsEmpty)
				{
					AttachedToParent();
				}
			}
		}

		void DetachedFromParent(ZGuid oldValue)
		{
			if (CY_ParentTableCode == AsycudaManifestHeaderSchema.Constants.Prefix)
			{
				var header = parentLoaders.LoadBusinessObject(Factory, CY_ParentTableCode, oldValue) as AsycudaManifestHeader;
				header?.Receptacles.SequenceNumberCalculator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			}
		}

		void AttachedToParent()
		{
			if (CY_ParentTableCode == AsycudaManifestHeaderSchema.Constants.Prefix)
			{
				((AsycudaManifestHeader)Parent)?.Receptacles.SequenceNumberCalculator.RecalculateWhenAdded(this);
			}
		}

		#endregion
	}
}
