namespace CargoWise.Windows.UI
{
	public interface IBindableControlExtension : IControlExtension
	{
		void SetDataBinding(object dataSource, string dataMember);
	}
}
