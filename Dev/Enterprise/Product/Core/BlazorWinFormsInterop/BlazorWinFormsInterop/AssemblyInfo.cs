using System.Runtime.CompilerServices;
using CargoWiseNext.Infrastructure.Authentication;
using CargoWiseNext.Infrastructure.Installations;
using WTG.StaticAnalysis.Annotation;

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.BlazorWinFormsInterop.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: UsesConstants(typeof(QueryParameters))]
[assembly: UsesConstants(typeof(UrlHandlers))]
#endif

