using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.US.ISF
{
	partial class DispositionCodeList : Integration.Customs.US.ISF.IDispositionCodeCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		#region ICodeDescriptionPairListProvider Members

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return this;
		}

		#endregion
	}
}
