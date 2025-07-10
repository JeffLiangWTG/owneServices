using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class HarmonisedCodeWrapperCollection : GenericWrapperCollection<HarmonisedCodeWrapper>
	{
		#region Constructors

		public HarmonisedCodeWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public HarmonisedCodeWrapperCollection(IEnumerable<IHarmonisedCode> harmonisedCodes, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (var harmonisedCode in harmonisedCodes)
			{
				Add(new HarmonisedCodeWrapper(harmonisedCode, Factory));
			}
		}

		#endregion
	}
}
