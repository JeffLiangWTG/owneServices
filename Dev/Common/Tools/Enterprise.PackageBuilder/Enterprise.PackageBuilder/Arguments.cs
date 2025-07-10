using System.Collections;
using System.IO;
using System.Reflection;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.PackageBuilder
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer only tool")]
	class Arguments : CommandLineArguments
	{
		public Arguments(string[] args)
			: base(args, PossibleOptions)
		{
			if (string.IsNullOrEmpty(DatabaseName))
			{
				DatabaseName = "Odyssey";
			}
			Validate(args);
		}

		public void Validate(string[] args)
		{
			if (string.IsNullOrEmpty(BuildPath) ^ IsDevBuild)
			{
				ThrowArgumentsException("You must use either -BuildPath: or -DevBuild", args);
			}

			if (!string.IsNullOrEmpty(EnterpriseCode) && IsBuildMaster)
			{
				ThrowArgumentsException("You cannot use both -EnterpriseCode: and -BuildMaster", args);
			}

			if (IsDevBuild && IsBuildMaster)
			{
				ThrowArgumentsException("You cannot use both -DevBuild and -BuildMaster", args);
			}
		}

		const string OptionTargetPath = "-TargetPath:";
		public string TargetPath
		{
			get { return (string)this[OptionTargetPath]; }
		}
		static string DefaultTargetPath
		{
			get
			{
				return Path.Combine(
					Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)),
					"package");
			}
		}

		const string OptionDevBuild = "-DevBuild";
		public bool IsDevBuild
		{
			get { return (bool)this[OptionDevBuild]; }
		}

		const string OptionBuildPath = "-BuildPath:";
		public string BuildPath
		{
			get { return (string)this[OptionBuildPath]; }
			set { this.OptionalArgs[OptionBuildPath] = value; }
		}

		const string OptionEnterpriseCode = "-EnterpriseCode:";
		public string EnterpriseCode
		{
			get { return (string)this[OptionEnterpriseCode]; }
		}

		const string OptionBuildMaster = "-BuildMaster";
		public bool IsBuildMaster
		{
			get { return (bool)this[OptionBuildMaster]; }
		}

		const string OptionDvd = "-Dvd";
		public bool IsDvd
		{
			get { return (bool)this[OptionDvd]; }
		}

		const string OptionDvdTemplate = "-DvdTemplate:";
		public string DvdTemplate
		{
			get { return (string)this[OptionDvdTemplate]; }
		}
		static string DefaultDvdTemplate
		{
			get { return @"\\datfiles.wtg.zone\DAT\ServerInstallCD\DvdBuilderFiles\v5"; }
		}

		const string OptionUseDbBackups = "-UseDbBackups:";
		public string UseDbBackups
		{
			get { return (string)this[OptionUseDbBackups]; }
		}
		static string DefaultDbBackups
		{
			get { return @"\\datfiles.wtg.zone\DAT\ServerInstallCD\databases\20130318"; }
		}

		static Hashtable PossibleOptions
		{
			get
			{
				Hashtable possibleOptions = new Hashtable();

				possibleOptions.Add(OptionTargetPath, DefaultTargetPath);
				possibleOptions.Add(OptionBuildPath, null);
				possibleOptions.Add(OptionDevBuild, false);
				possibleOptions.Add(OptionEnterpriseCode, null);
				possibleOptions.Add(OptionBuildMaster, false);
				possibleOptions.Add(OptionDvd, false);
				possibleOptions.Add(OptionDvdTemplate, DefaultDvdTemplate);
				possibleOptions.Add(OptionUseDbBackups, DefaultDbBackups);

				return possibleOptions;
			}
		}
	}
}
