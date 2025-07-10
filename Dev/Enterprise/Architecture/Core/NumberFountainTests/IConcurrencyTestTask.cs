namespace Enterprise.NumberFountain.Testing
{
	/// <summary>
	/// A task that can be used with <see cref="FountainTestUtils.TestConcurrency{TResult}"/>.
	/// </summary>
	public interface IConcurrencyTestTask<TResult>
	{
		/// <summary>
		/// Initialization method that can create thread-specific resources, such as database connections.
		/// </summary>
		void Init();

		/// <summary>
		/// Main method that should be tested.
		/// </summary>
		TResult Run(int threadNum);

		/// <summary>
		/// Method that should clean up disposable resources.
		/// </summary>
		void Cleanup();
	}
}
