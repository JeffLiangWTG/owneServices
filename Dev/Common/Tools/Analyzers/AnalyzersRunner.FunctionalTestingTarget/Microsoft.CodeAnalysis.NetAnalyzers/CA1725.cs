namespace AnalyzersRunner.FunctionalTestingTarget.Microsoft.CodeAnalysis.NetAnalyzers
{
	/// <summary>
	/// Rule: Parameter names should match base declaration
	/// </summary>
	public interface ICA1725Example
	{
		void Method(int firstParameter);
	}

	public abstract class CA1725 : ICA1725Example
	{
		// CA1725: Parameter names should match base declaration
		public void Method(int firstParam)
		{
			// method implementation
		}

		protected abstract void Method2(int secondParameter);

		class CA1725Private : CA1725
		{
			protected override void Method2(int secondParam)
			{
				throw new System.NotImplementedException();
			}
		}
	}

	/// <summary>
	/// Rule: Parameter names should match base declaration
	/// </summary>
	class CA1725Internal : ICA1725Example
	{
		// CA1725: Parameter names should match base declaration
		public void Method(int firstParam)
		{
			// method implementation
		}
	}
}
