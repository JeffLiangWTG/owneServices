using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class AdditionalElementWrapperCollection : NonPersistentBusinessObjectCollection<AdditionalElementWrapper>, IAdditionalElementCollection
	{
		public AdditionalElementWrapperCollection(AdditionalInformationWrapper parent) : base(parent.Factory)
		{
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.InvalidOperationException();
		}

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
		protected override bool AllowSort => false;

		public AdditionalElementWrapper this[string code]
		{
			get { return this.Cast<AdditionalElementWrapper>().FirstOrDefault(x => x.ElementCode == code); }
		}

		public ZString GetValue(ZString code)
		{
			return this[code]?.ElementValue.Trim() ?? ZString.Empty;
		}

		public void SetValue(ZString code, ZString value)
		{
			var additionalElementWrapper = this[code];
			if (additionalElementWrapper != null)
			{
				additionalElementWrapper.ElementValue = value.Trim().Left(additionalElementWrapper.ElementValueInfo.MaxLength);
			}
		}

		#endregion
	}
}
