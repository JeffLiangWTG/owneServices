namespace Enterprise.DataTransfer.Native.Common.Behaviours
{
	interface IEntityBehaviour
	{
		bool CanBeAppliedToAction(EntityAction action);
		void Apply(BehaviourContext behaviourContext);
	}
}
