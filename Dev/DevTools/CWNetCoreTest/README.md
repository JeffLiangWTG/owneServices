## Rules for writing Custom Test Adapter

1. Create a project that is a class library
2. **Very important** : The assembly name **must** end with **.TestAdapter** or vstest.console.exe will not recognize your test adapter. You can change this setting in the properties of your project.
3. Import **Microsoft.VisualStudio.TestPlatform.ObjectModel** or add it as PackageReference.
4. You only need to implement **ITestDiscoverer** and **ITestExecutor**
5. If your test adapter is for .dll or .exe files the default test adapter will run first.
6. If following error occurs while running testcases _An exception occurred while invoking executor 'executor://cw_netcore_testexecutor/': A task was canceled_
  , make sure CW1 app runs proper in the local , if not restore DB and try again.


**References**
	1. https://stackoverflow.com/questions/38100057/how-to-create-and-install-test-adapter-in-visual-studio/41069143#41069143
	2. https://github.com/microsoft/vstest-docs/blob/main/RFCs/0004-Adapter-Extensibility.md#specifying-an-adapter
