using System.Globalization;
using CargoWise.Types;

namespace Enterprise.Initialisation
{
	public class CultureProvider : ICultureProvider
	{
		CultureInfo ICultureProvider.Culture
		{
			get { return Enterprise.ZArchitecture.Core.Culture.Current; }
		}
	}
}
