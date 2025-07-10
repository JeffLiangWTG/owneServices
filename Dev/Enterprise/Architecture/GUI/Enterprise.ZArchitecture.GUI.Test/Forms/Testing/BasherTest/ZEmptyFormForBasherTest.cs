namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZEmptyFormForBasherTest : ZForm
	{
		public ZEmptyFormForBasherTest()
		{
			InitializeForm();
		}

		public ZEmptyFormForBasherTest(object dataSource)
			: base(dataSource)
		{
			InitializeForm();
		}

		void InitializeForm()
		{
		}
	}
}
