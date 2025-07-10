using System.Collections.Generic;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPEJobDeclarationRatingAdaptersProvider : RatingAdaptersProvider<UPEJobDeclaration>
	{
		public UPEJobDeclarationRatingAdaptersProvider(UPEJobDeclaration parent) : base(parent) { }

		protected override List<IAutoRating> GetAdapters(UPEJobDeclaration parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			return new List<IAutoRating> { new UPEJobDeclarationRatingAdapter<UPEJobDeclaration>(parent) };
		}
	}
}
