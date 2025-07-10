using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.DE.NCTS.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.EuNctsUnloadingRemark)]
	public class UnloadingRemarkAddInfo : EU.NCTS.Business.UnloadingRemarkAddInfo
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public UnloadingRemarkAddInfo(BusinessObjectFactory factory)
			: base(factory)
		{
			SetReadOnlyIncludingChildren();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public UnloadingRemarkAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
			SetReadOnlyIncludingChildren();
		}

		protected override EU.NCTS.Business.UnloadingRemarkAddInfoLookups GetNewLookups()
		{
			return new UnloadingRemarkAddInfoLookups(this);
		}

		public new UnloadingRemarkAddInfoLookups Lookups => (UnloadingRemarkAddInfoLookups)base.Lookups;

		public new CusAddInfoUnloadingRemarkAddInfo Parent => (CusAddInfoUnloadingRemarkAddInfo)base.Parent;

		NctsHeader Header => Parent.Parent as NctsHeader;

		public override ZString G9_Conform
		{
			get => base.G9_Conform;
			set
			{
				var oldValue = G9_Conform;
				base.G9_Conform = value;
				var header = Header;
				if (oldValue != G9_Conform && !IsCopying && header != null)
				{
					header.UnloadingMovementHeader.GoodsItems.SetReadOnlyIncludingChildren(IsUnloadingConformAnswerYes);
					header.UnloadingMovementHeader.GoodsItems.RefreshBinding();
					header.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString G9_StateOfSealsOk
		{
			get => base.G9_StateOfSealsOk;
			set
			{
				var oldValue = G9_StateOfSealsOk;
				base.G9_StateOfSealsOk = value;
				var header = Header;
				if (oldValue != G9_StateOfSealsOk && !IsCopying && header != null)
				{
					var isStateOfSealsOk = IsStateOfSealsOkAnswerYes;
					if (isStateOfSealsOk)
					{
						header.DepartureHeaderContainers.RemoveAndDeleteAll();
					}
					header.DepartureHeaderContainers.SetReadOnlyIncludingChildren(isStateOfSealsOk);
					header.DepartureHeaderContainers.RefreshBinding();
					header.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString G9_UnloadingCompletion
		{
			get => base.G9_UnloadingCompletion;
			set
			{
				base.G9_UnloadingCompletion = value;
				Header?.MarkAsNeedingValidation();
			}
		}

		public override ZDateTime G9_UnloadingDate
		{
			get => base.G9_UnloadingDate;
			set
			{
				base.G9_UnloadingDate = value;
				Header?.MarkAsNeedingValidation();
			}
		}

		public override ZString G9_UnloadingRemark
		{
			get => base.G9_UnloadingRemark;
			set
			{
				base.G9_UnloadingRemark = value;
				Header?.MarkAsNeedingValidation();
			}
		}

		void SetReadOnlyIncludingChildren()
		{
			var header = Header;
			if (header != null)
			{
				header.UnloadingMovementHeader.GoodsItems.SetReadOnlyIncludingChildren(IsUnloadingConformAnswerYes);
				header.DepartureHeaderContainers.SetReadOnlyIncludingChildren(IsStateOfSealsOkAnswerYes);
			}
		}

		ZBool IsUnloadingConformAnswerYes => G9_Conform == YesNoList.Codes.Yes;

		ZBool IsStateOfSealsOkAnswerYes => G9_StateOfSealsOk == YesNoList.Codes.Yes;
	}
}
