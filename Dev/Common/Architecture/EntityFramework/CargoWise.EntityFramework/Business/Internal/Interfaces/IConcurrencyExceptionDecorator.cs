using System.Text;

namespace CargoWise.EntityFramework
{
	public interface IConcurrencyExceptionDecorator
	{
		void AppendDecoratedDisplayName(StringBuilder stringBuilder, IPropertyRecord record);
	}
}
