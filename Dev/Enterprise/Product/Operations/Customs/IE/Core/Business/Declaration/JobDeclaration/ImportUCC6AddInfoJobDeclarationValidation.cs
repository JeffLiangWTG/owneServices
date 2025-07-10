using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public partial class ImportUCC6JobDeclarationValidation
	{
		protected override void CheckJE_BorderTransportMeans()
		{
			var declaration = Parent;
			var targetInfo = Parent.ZG_BorderTransportMeansInfo;

			if (!declaration.JE_TransportMode.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
		}
	}
}
