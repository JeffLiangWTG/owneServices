namespace Enterprise.Customs.CA.Business
{
	partial class ACROSSServiceOptions : Integration.Customs.CA.IACROSSServiceOptionsCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		#region ICodeDescriptionPairListProvider Members

		public ZArchitecture.Core.ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return this;
		}

		#endregion

	}
}
