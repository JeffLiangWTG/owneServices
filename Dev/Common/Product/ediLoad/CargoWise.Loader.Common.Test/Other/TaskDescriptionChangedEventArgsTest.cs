using NUnit.Framework;

namespace CargoWise.Loader.Common.Testing
{
	class TaskDescriptionChangedEventArgsTest : TestCase
	{
		public void TestConstructorAndProperty()
		{
			AssertEquals("Some description", new TaskDescriptionChangedEventArgs("Some description").TaskDescription);
		}
	}
}