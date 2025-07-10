using System.Reflection;
using System.Runtime.CompilerServices;
using Enterprise.Messaging.Business;
using WTG.StaticAnalysis.Annotation;

[assembly: AssemblyTitle("DE Customs EMCS Messaging")]
[assembly: AssemblyDescription("DE Customs EMCS Messaging")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.DE.EMCS.Messaging.Test, PublicKey = " + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: UsesConstants(typeof(EDIInterchange.ApplicationCodes))]
