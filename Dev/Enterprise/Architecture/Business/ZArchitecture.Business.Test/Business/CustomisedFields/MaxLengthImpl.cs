using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class MaxLengthImpl : DynamicMetaData
	{
		public MaxLengthImpl(int maxLength) : base(MetaDataTypes.MaxLength, maxLength) { }
	}
}
