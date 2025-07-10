using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EU
		{
			public static partial class NCTS
			{
				public interface ICusInBondHeader : Customs.ICusInBondHeader
				{
					ZBool IsSecurityDeclaration { get; }
					ZBool IsInPhase5TransitionPeriod { get; }
					void Synchronise(ZBool force);
					void SetMovementType(ZString headerType);
				}

				public interface ICusInBondMoveHeader : Customs.ICusInBondMoveHeader
				{
				}
			}
		}
	}
}
