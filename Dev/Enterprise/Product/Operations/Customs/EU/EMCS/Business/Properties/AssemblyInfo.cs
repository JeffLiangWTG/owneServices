using System.Reflection;
using System.Runtime.CompilerServices;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

[assembly: AssemblyTitle("EU Customs EMCS Business")]
[assembly: AssemblyDescription("EU Customs EMCS Business")]
[assembly: UniversalCopyAddInfoPropertyDefinition(typeof(AutoEUEMCSAddInfo.Schema), Enterprise.Core.Constants.CountryCodes.Latvia)]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.EU.EMCS.Business.Test, PublicKey = " + CommonAssemblyInfo.PublicKey)]
#endif

[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.IE.Business",
	"Enterprise.Customs.IE.Business.Test",
	"Enterprise.Customs.IE.GUI",
	"Enterprise.Customs.IE.GUI.Test",
	"Enterprise.Customs.IE.ServiceTasks.Test",
	"Enterprise.Customs.IE.EMCS.Business",
	"Enterprise.Customs.IE.EMCS.Business.Test",
	"Enterprise.Customs.IE.EMCS.GUI",
	"Enterprise.Customs.IE.EMCS.GUI.Test",
	"Enterprise.Customs.GB.Business.Test",
	"Enterprise.Customs.GB.EMCS.Business",
	"Enterprise.Customs.GB.EMCS.Business.Test",
	"Enterprise.Customs.GB.EMCS.GUI",
	"Enterprise.Customs.GB.EMCS.GUI.Test",
	"Enterprise.Customs.GB.EMCS.ServiceTasks",
	"Enterprise.Customs.DE.EMCS.Business",
	"Enterprise.Customs.DE.EMCS.Business.Test",
	"Enterprise.Customs.EU.EMCS.Business.XmlSerializers",
	"Enterprise.Customs.EU.EMCS.DataTransfer",
	"Enterprise.Customs.EU.EMCS.GUI",
	"Enterprise.Customs.EU.EMCS.Module",
	"Enterprise.Customs.EU.EMCS.Business.Test",
	"Enterprise.Customs.EU.EMCS.DataTransfer.Test",
	"Enterprise.Customs.EU.EMCS.GUI.Test",
	"Enterprise.Customs.EU.EMCS.Module.Test",
	"DocumentWrappers",
	"DocumentWrappers.Test")]
