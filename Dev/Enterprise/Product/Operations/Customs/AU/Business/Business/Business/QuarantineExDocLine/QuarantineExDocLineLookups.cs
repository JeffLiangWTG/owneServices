//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoQuarantineExDocLineLookups
//
//    This class should be used for overriding collections in AutoQuarantineExDocLineLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineExDocLineLookups : AutoQuarantineExDocLineLookups
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "This class should not be used and this is why there is an exception to make it explicit. We need to turn the exception off for the class being used.")]
		public QuarantineExDocLineLookups(AutoQuarantineExDocLine parent)
			: base(parent)
		{
			Setup();
		}

		protected virtual void Setup()
		{
			throw new System.NotImplementedException("This is not the base class to use. Use CommonQuarantineExDocLineLookups");
		}
	}
}
