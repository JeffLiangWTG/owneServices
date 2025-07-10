This service is manually deployed. There is no automatic deployment or upgrade.
Generate the deployed files from the "dotnet publish" command via the VS -> View -> Terminal
For a release build, first build the referenced DLLs from CargoWise One in release mode.

Debug Build:
PS \ProductRegistration> cd Service
PS \Service> dotnet publish /p:PublishProfile=ToLocalFolderVS2022 -c:Debug -f net48 /p:PublishDir=C:\temp\publish\

Release Build:
PS \ProductRegistration> cd Service
PS \Service> dotnet publish /p:PublishProfile=ToLocalFolderVS2022 -c:Release -f net48 /p:PublishDir=C:\temp\publish\

Publish will put the files to deploy in a folder like: C:\temp\publish\
The contents of the folder can now be copied to the physical folder of the production service.
Information Services team can do this if you don't have access the the production web server(s).



