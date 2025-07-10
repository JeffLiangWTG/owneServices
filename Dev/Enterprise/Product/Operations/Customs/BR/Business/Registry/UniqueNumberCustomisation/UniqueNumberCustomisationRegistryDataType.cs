using System;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.BR.Registry
{
	public sealed class UniqueNumberCustomisationRegistryDataType : BillCustomisationRegistryDataType
	{
		public UniqueNumberCustomisationRegistryDataType() : this(true)
		{
		}

		public UniqueNumberCustomisationRegistryDataType(bool supportDirection) : base(new UniqueNumberCustomisation())
		{
			SupportsDirection = supportDirection;
		}

		public new UniqueNumberCustomisation DefaultValue => base.DefaultValue as UniqueNumberCustomisation;

		public bool SupportsDirection
		{
			get => DefaultValue.SupportsDirection;
			set => DefaultValue.SupportsDirection = value;
		}

		protected override Type DataTypeCore => typeof(UniqueNumberCustomisation);

		protected override BillOfLadingNumberCustomisation DeserialiseCore(byte[] value)
		{
			var billOfLadingNumberCustomisation = (UniqueNumberCustomisation)base.DeserialiseCore(value);
			billOfLadingNumberCustomisation.SupportsDirection = SupportsDirection;
			return billOfLadingNumberCustomisation;
		}
	}
}
