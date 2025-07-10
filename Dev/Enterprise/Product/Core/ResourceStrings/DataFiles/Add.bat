mkdir %1
copy Template\ResourcesDelta.xml %1\ResourcesDelta.xml
mkdir %1\Properties

call BatchSubstitute.bat XXX %1 Template\Resources.xml > %1\Resources.xml

tf add %1\Resources.xml
tf add %1\ResourcesDelta.xml
