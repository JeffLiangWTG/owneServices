using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase
{
	public class EUMemberStateCommunicationCollection<T> : DependentBusinessObjectCollection<T, BusinessObject>
		where T : EUMemberStateCommunication
	{
		public EUMemberStateCommunicationCollection(BusinessObject parent) : base(parent)
		{
		}

		protected sealed override string FkColumnName => EUMemberStateCommunicationSchema.Constants.EUS_ParentId;
	}
}
