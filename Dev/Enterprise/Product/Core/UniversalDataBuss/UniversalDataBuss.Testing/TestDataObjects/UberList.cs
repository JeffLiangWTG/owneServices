using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects
{
	[CollectionAttributes("Style")]
	public class UberList<T> : List<T> where T : IDataObject
	{
		public CollectionStyle? Style { get; set; }
	}

	public enum CollectionStyle { Classic, Modern, Retro }
}
