using System.Collections.Generic;

namespace Enterprise.Dat.Implementation.Preconditions
{
	abstract class DependentPrecondition : IPrecondition
	{
		string errorMessage = string.Empty;

		protected DependentPrecondition()
		{
		}

		protected abstract IEnumerable<IPrecondition> DependentPreconditions { get; }

		public string ErrorMessage
		{
			get { return errorMessage; }
		}

		public bool CheckPreconditionMet()
		{
			foreach (IPrecondition condition in DependentPreconditions)
			{
				bool conditionMet = condition.CheckPreconditionMet();
				if (!conditionMet)
				{
					errorMessage = condition.ErrorMessage;
					return false;
				}
			}

			return true;
		}
	}
}
