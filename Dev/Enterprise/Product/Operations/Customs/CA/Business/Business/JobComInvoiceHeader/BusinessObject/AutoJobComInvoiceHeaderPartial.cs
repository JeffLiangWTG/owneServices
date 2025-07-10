namespace Enterprise.Customs.CA.Business
{
	public abstract partial class AutoJobComInvoiceHeader
	{
		#region AddInfo object/Validation and Lookups objects

		public override void OnLoaded()
		{
			bool oldHasChanges = AddInfo.HasChanges;
			try
			{
				base.OnLoaded();
				AddInfo.LoadPropertiesFromString(JZ_AddInfo);
			}
			finally
			{
				AddInfo.HasChanges = oldHasChanges;
			}
		}

		internal AddInfoJobComInvoiceHeader GetAddInfo()
		{
			return AddInfo;
		}

		#endregion
	}
}
