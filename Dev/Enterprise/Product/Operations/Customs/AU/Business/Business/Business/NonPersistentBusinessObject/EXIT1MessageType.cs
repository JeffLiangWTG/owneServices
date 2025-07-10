using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXIT1MessageType : NonPersistentBusinessObject, IObsoleteValidation
	{
		public EXIT1MessageType()
		{
		}

		#region Schema

		public static class Schema
		{
			public const string ZX_IsConfirming = "ZX_IsConfirming";
			public const string ZX_IsConfirmed = "ZX_IsConfirmed";
		}

		#endregion

		#region Properties

		protected ZBool fZX_IsConfirming;
		public ZBool ZX_IsConfirming
		{
			get { return fZX_IsConfirming; }
			set { SetNonPersistentPropertyValue(ZX_IsConfirmingInfo, ref fZX_IsConfirming, value); }
		}

		public ZPropertyInfo ZX_IsConfirmingInfo
		{
			get { return GetZPropertyInfo(Schema.ZX_IsConfirming); }
		}

		protected ZBool fZX_IsConfirmed;
		public ZBool ZX_IsConfirmed
		{
			get { return fZX_IsConfirmed; }
			set { SetNonPersistentPropertyValue(ZX_IsConfirmedInfo, ref fZX_IsConfirmed, value); }
		}

		public ZPropertyInfo ZX_IsConfirmedInfo
		{
			get { return GetZPropertyInfo(Schema.ZX_IsConfirmed); }
		}

		#endregion

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ZX_IsConfirmed = true;
		}

		#endregion
	}
}
