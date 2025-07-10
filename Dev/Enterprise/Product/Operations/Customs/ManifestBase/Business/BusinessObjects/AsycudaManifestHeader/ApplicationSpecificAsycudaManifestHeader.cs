using System.ComponentModel;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ManifestBase
{
	public abstract class ApplicationSpecificAsycudaManifestHeader : AsycudaManifestHeader
	{
		protected ApplicationSpecificAsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			base.AMA_ApplicationCode = GetApplicationCode();
		}

		[BusinessObjectTestExclude]
		[ReadOnly(true)]
		public override ZString AMA_ApplicationCode
		{
			get => base.AMA_ApplicationCode;
			set
			{
				var oldValue = AMA_ApplicationCode;
				base.AMA_ApplicationCode = value;
				if (!IsCopying && oldValue != AMA_ApplicationCode)
				{
					var typeName = GetType().FullName;
					ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, typeName + "-AMA_ApplicationCode"), string.Format(CultureInfo.InvariantCulture, "{2} Application Code was changed from '{0}' to '{1}'", oldValue, AMA_ApplicationCode, typeName));
				}
			}
		}

		protected abstract ZString GetApplicationCode();
	}
}
