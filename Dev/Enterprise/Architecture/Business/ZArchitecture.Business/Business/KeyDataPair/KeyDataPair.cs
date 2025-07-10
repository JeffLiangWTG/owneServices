using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business
{
	public class KeyDataPair : AutoKeyDataPair, IKeyDataPair
	{
		public override bool ReadOnly
		{
			get { return true; }
			set { }
		}
	}
}
