using WTG.StaticAnalysis.Annotation;

namespace Enterprise.UniversalDataBuss.Integration
{
	[Immutable]
	public class UniversalXmlSchema : IUniversalXmlSchema
	{
		UniversalXmlSchema(string @namespace, string version)
		{
			this.@namespace = @namespace;
			this.version = version;
		}

		/// <summary>
		/// This schema was never released to customers and should not be used. Use Version_2011_11 instead
		/// </summary>
		public static UniversalXmlSchema Version_2012_11_DO_NOT_USE
		{
			get { return version_2012_11; }
		}

		public static UniversalXmlSchema Version_2011_11
		{
			get { return version_2011_11; }
		}

		public string Namespace
		{
			get { return @namespace; }
		}

		public string Version
		{
			get { return version; }
		}

		static readonly UniversalXmlSchema version_2012_11 = new UniversalXmlSchema(UniversalXmlInfo.Namespace_2012_11, UniversalXmlInfo.Version_2012_11_DO_NOT_USE);
		static readonly UniversalXmlSchema version_2011_11 = new UniversalXmlSchema(UniversalXmlInfo.Namespace_2011_11, UniversalXmlInfo.Version_2011_11);
		readonly string @namespace;
		readonly string version;
	}
}
